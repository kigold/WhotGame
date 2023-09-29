using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using WhotGame.Core.Data.Models;
using WhotGame.Core.Data.Repositories;
using WhotGame.Core.DTO.Requests;
using WhotGame.Core.DTO.Response;
using WhotGame.Silo.ViewModels;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace WhotGame.Silo.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthorizationController : BaseController
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IRepository<User> _userRepo;

        public AuthorizationController(UserManager<User> userManager, 
            SignInManager<User> signInManager, 
            RoleManager<Role> roleManager, 
            IOpenIddictApplicationManager applicationManager,
            IRepository<User> userRepo, 
            IHttpContextAccessor httpContext)
            :base(httpContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _applicationManager = applicationManager;
            _roleManager = roleManager;
            _userRepo = userRepo;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser(CreateUserRequest request)
        {
            if (await _userManager.FindByNameAsync(request.Email) is not null)
                return BadRequest("User already exists");

            var user = new User
            {
                Email = request.Email,
                UserName = request.Email,
                Firstname = request.Firstname,
                Lastname = request.Lastname,
                Avatar = request.Avatar,
            };

            var hash = _userManager.PasswordHasher.HashPassword(user, "P@ssw0rd");
            user.PasswordHash = hash;
            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors.First());

            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: new UserResponse(user.Id, user.Email, user.Firstname, user.Avatar));
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            //TODO Paginate
            var users = _userRepo.Get().ToList();

            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: users.Select(x => new UserResponse(x.Id, x.Email, x.Firstname, x.Avatar)));
        }

        [HttpPost("~/connect/token"), Produces("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            ClaimsPrincipal claimsPrincipal;

            if (request.IsPasswordGrantType())
                return await TokensForPasswordGrantType(request);

            if (request.IsClientCredentialsGrantType())
            {
                // Note: the client credentials are automatically validated by OpenIddict:
                // if client_id or client_secret are invalid, this action won't be invoked.

                var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                    throw new InvalidOperationException("The application cannot be found.");

                // Create a new ClaimsIdentity containing the claims that
                // will be used to create an id_token, a token or a code.
                var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

                // Use the client_id as the subject identifier.
                identity.AddClaim(Claims.Subject,
                    await _applicationManager.GetClientIdAsync(application),
                    Destinations.AccessToken, Destinations.IdentityToken);

                identity.AddClaim(Claims.Name,
                    await _applicationManager.GetDisplayNameAsync(application),
                    Destinations.AccessToken, Destinations.IdentityToken);

                claimsPrincipal = new ClaimsPrincipal(identity);
            }
            else if (request.IsAuthorizationCodeGrantType())
            {
                // Retrieve the claims principal stored in the authorization code
                claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            }
            else if (request.IsRefreshTokenGrantType())
            {
                var info = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                var user = await _userManager.GetUserAsync(info.Principal);
                if (user == null)
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The refresh token is no longer valid."
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                if (!await _signInManager.CanSignInAsync(user))
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }
                var identity = new ClaimsIdentity(
                    TokenValidationParameters.DefaultAuthenticationType,
                    OpenIddictConstants.Claims.Name,
                    OpenIddictConstants.Claims.Role);

                AddUserClaims(user, identity);
                // Add more claims if necessary

                foreach (var userRole in await _userManager.GetRolesAsync(user))
                {
                    identity.AddClaim(OpenIddictConstants.Claims.Role, userRole, OpenIddictConstants.Destinations.AccessToken);
                }

                claimsPrincipal = new ClaimsPrincipal(identity);
                claimsPrincipal.SetScopes(new string[]
                {
                    OpenIddictConstants.Scopes.Roles,
                    OpenIddictConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    "api"
                });
            }
            else
            {
                throw new NotImplementedException("The specified grant is not implemented.");
            }

            //return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private async Task<IActionResult> TokensForPasswordGrantType(OpenIddictRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                return Unauthorized();

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (signInResult.Succeeded)
            {
                var identity = new ClaimsIdentity(
                    TokenValidationParameters.DefaultAuthenticationType,
                    OpenIddictConstants.Claims.Name,
                    OpenIddictConstants.Claims.Role);

                AddUserClaims(user, identity);
                // Add more claims if necessary

                foreach (var userRole in await _userManager.GetRolesAsync(user))
                {
                    identity.AddClaim(OpenIddictConstants.Claims.Role, userRole, OpenIddictConstants.Destinations.AccessToken);
                }

                var claimsPrincipal = new ClaimsPrincipal(identity);
                claimsPrincipal.SetScopes(new string[]
                {
                    OpenIddictConstants.Scopes.Roles,
                    OpenIddictConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    "api"
                });

                return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            else
                return Unauthorized();
        }

        private static void AddUserClaims(User user, ClaimsIdentity identity)
        {
            identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString(), OpenIddictConstants.Destinations.AccessToken);
            identity.AddClaim(OpenIddictConstants.Claims.Username, user.UserName, OpenIddictConstants.Destinations.AccessToken);
            identity.AddClaim(OpenIddictConstants.Claims.Name, user.FullName, OpenIddictConstants.Destinations.AccessToken);
            identity.AddClaim(OpenIddictConstants.Claims.Email, user.Email, OpenIddictConstants.Destinations.AccessToken);
            identity.AddClaim("avatar", user.Avatar, OpenIddictConstants.Destinations.AccessToken);
        }

    }
}
