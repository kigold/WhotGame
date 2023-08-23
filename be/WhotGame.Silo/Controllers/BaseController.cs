using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WhotGame.Silo.ViewModels;

namespace WhotGame.Silo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContext;

        public BaseController(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }

        public IActionResult ApiResponse<T>(T data = default, string message = "",
            ApiResponseCodes codes = ApiResponseCodes.OK, int? totalCount = 0, params string[] errors)
        {
            var response = new ApiResponse<T>(data, message, codes, totalCount, errors);
            response.Description = message ?? response.Code.ToString();
            return codes == ApiResponseCodes.OK ? Ok(response) : BadRequest(response);
        }

        public async Task<IActionResult> Process<T>(Func<Task<ResultModel<T>>> request)
        {
            var result = await request.Invoke();

            if (result.HasError)
                return ApiResponse<string>(errors: result.ErrorMessages.ToArray());

            if (result.HasError)
                return ApiResponse<string>(errors: result.ErrorMessages.ToArray());

            Type t = result?.Data?.GetType();
            var countProperty = t.GetProperty("Count");
            int count = (int?)countProperty?.GetValue(result.Data) ?? 0;

            return ApiResponse(message: string.IsNullOrEmpty(result.Message) ? "Success" : result.Message, codes: ApiResponseCodes.OK, data: result.Data, totalCount: count);
        }

        [NonAction]
        public UserPrincipal GetCurrentUser()
        {
            if (_httpContext.HttpContext != null && _httpContext.HttpContext.User != null)
            {
                return new UserPrincipal(_httpContext.HttpContext.User);
            }

            return null;
        }

        public class UserPrincipal : ClaimsPrincipal
        {
            public UserPrincipal(ClaimsPrincipal principal) : base(principal)
            {
            }

            private string GetClaimValue(string key)
            {
                var identity = Identity as ClaimsIdentity;
                if (identity == null)
                    return null;

                var claim = identity.Claims.FirstOrDefault(c => c.Type == key);
                return claim?.Value;
            }

            public string Email
            {
                get
                {
                    if (FindFirst(OpenIdConnectConstants.Claims.Email) == null)
                        return string.Empty;

                    return GetClaimValue(OpenIdConnectConstants.Claims.Email);
                }
            }

            public string Username
            {
                get
                {
                    if (FindFirst(OpenIdConnectConstants.Claims.Username) == null)
                        return string.Empty;

                    return GetClaimValue(OpenIdConnectConstants.Claims.Username);
                }
            }

            public long UserId
            {
                get
                {
                    if (FindFirst(OpenIdConnectConstants.Claims.Subject) == null)
                        return default;

                    return Convert.ToInt64(GetClaimValue(OpenIdConnectConstants.Claims.Subject));
                }
            }

            public string Name
            {
                get
                {
                    var usernameClaim = FindFirst(OpenIdConnectConstants.Claims.Name);

                    if (usernameClaim == null)
                        return string.Empty;

                    return usernameClaim.Value;
                }
            }
        }
    }
}
