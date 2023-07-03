using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using WhotGame.Abstractions.Models;
using WhotGame.Core.Data.Models;

namespace WhoteGame.Services
{
    public class GameHub : Hub
    {
        private string GetUserFromContext()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext != null)
            {
                var jwtToken = httpContext.Request.Query["access_token"];
                var handler = new JwtSecurityTokenHandler();
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    var token = handler.ReadJwtToken(jwtToken);
                    var tokenS = token as JwtSecurityToken;

                    // replace email with your claim name
                    //var email = tokenS.Claims.First(claim => claim.Type == "email").Value;
                    var userId = tokenS.Claims.First(claim => claim.Type == "sub").Value;
                    //if (jti != null && jti != "")
                    //{
                    //    Groups.AddToGroupAsync(Context.ConnectionId, jti);
                    //}
                    return userId;
                }
            }
            return "";
        }
        public override async Task OnConnectedAsync()
        {
            var userId = GetUserFromContext(); 
            await base.OnConnectedAsync();
        }

        public async Task StartGame(long gameId)
        {
            var playerGameConnection = GeneratePlayerGameConnection(gameId, int.Parse(GetUserFromContext()));
            var gameConnection = GenerateGameConnection(gameId);
            await Groups.AddToGroupAsync(Context.ConnectionId, playerGameConnection);
            await Groups.AddToGroupAsync(Context.ConnectionId, gameConnection);
            await Clients.Client(Context.ConnectionId).SendAsync("LoadGame", "Game Loading . . .", (60 * 60));
        }

        public static async Task BroadcastGameLog(IHubContext<GameHub> context, long gameId, string message)
        {
            await context.Clients.Group(GenerateGameConnection(gameId)).SendAsync("GameLog", message);
        }

        public static async Task BroadcastStartGame(IHubContext<GameHub> context, long gameId, PlayerLite player)
        {
            await context.Clients.Group(GenerateGameConnection(gameId)).SendAsync("StartGame", player);
        }

        public static async Task BroadcastUpdateTurn(IHubContext<GameHub> context, long gameId, PlayerLite player)
        {
            await context.Clients.Group(GenerateGameConnection(gameId)).SendAsync("UpdateTurn", player);
        }

        public static async Task BroadcastNewGame(IHubContext<GameHub> context, Game game)
        {
            await context.Clients.All.SendAsync("NewGame", game);
        }

        public static string GeneratePlayerGameConnection(long gameId, long playerId)
        {
            return $"connect_playergame_{gameId}_{playerId}";
        }

        public static string GenerateGameConnection(long gameId)
        {
            return $"connect_game_{gameId}";
        }
    }
}
