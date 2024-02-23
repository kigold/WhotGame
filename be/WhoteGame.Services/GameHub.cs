using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using WhotGame.Abstractions.Models;
using WhotGame.Core.DTO.Response;

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

        public async Task InitGame(long gameId)
        {
            var playerGameConnection = GeneratePlayerGameConnection(gameId, int.Parse(GetUserFromContext()));
            var gameConnection = GenerateGameConnection(gameId);
            await Groups.AddToGroupAsync(Context.ConnectionId, playerGameConnection);
            await Groups.AddToGroupAsync(Context.ConnectionId, gameConnection);
            await Clients.Client(Context.ConnectionId).SendAsync("LoadGame", $"Initing Loading GAME: {gameId}  . . .", (60 * 60));
        }

        /// <summary>
        /// Set ForAudience to view the game and not participate
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        public async Task InitGameLog(long gameId)
        {
            var gameConnection = GenerateGameConnection(gameId);
            await Groups.AddToGroupAsync(Context.ConnectionId, gameConnection);
            await Clients.Client(Context.ConnectionId).SendAsync("LoadGameLogs", $"Initing GAME Logs: {gameId}  . . .", (60 * 60));
        }

        public async Task SeekGame()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("LoadGame", "Game Loading . . .", (60 * 60));
        }

        public static async Task BroadcastGameLog(IHubContext<GameHub> context, long gameId, GameLog message)
        {
            await context.Clients.Group(GenerateGameConnection(gameId)).SendAsync("GameLog", message);
        }

        public static async Task BroadcastStartGame(IHubContext<GameHub> context, long gameId)
        {
            await context.Clients.Group(GenerateGameConnection(gameId)).SendAsync("StartGame", gameId);
        }

        public static async Task BroadcastEndGame(IHubContext<GameHub> context, long gameId)
        {
            await context.Clients.Group(GenerateGameConnection(gameId)).SendAsync("EndGame", gameId);
        }

        public static async Task BroadcastAbortGame(IHubContext<GameHub> context, long gameId)
        {
            await context.Clients.Group(GenerateGameConnection(gameId)).SendAsync("AbortGame", gameId);
        }

        public static async Task BroadcastUpdateTurn(IHubContext<GameHub> context, long gameId, PlayerLite player)
        {
            await context.Clients.Group(GenerateGameConnection(gameId)).SendAsync("UpdateTurn", player);
        }

        public static async Task BroadcastCardPlayed(IHubContext<GameHub> context, long gameId, CardResponse card)
        {
            await context.Clients.Group(GenerateGameConnection(gameId)).SendAsync("CardPlayed", card);
        }        
        
        public static async Task BroadcastSyncCardsForAuto(IHubContext<GameHub> context, long gameId, long playerId, CardResponse[] cards) //Sync Cards for auto response players
        {
            await context.Clients.Group(GeneratePlayerGameConnection(gameId, playerId)).SendAsync("SyncCardsForAuto", cards);
        }

        public static async Task BroadcastReceivedCards(IHubContext<GameHub> context, long gameId, long playerId, CardResponse[] cards)
        {
            await context.Clients.Group(GeneratePlayerGameConnection(gameId, playerId)).SendAsync("ReceivedCards", cards);
        }

        public static async Task BroadcastNewGame(IHubContext<GameHub> context, GameResponse game)
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
