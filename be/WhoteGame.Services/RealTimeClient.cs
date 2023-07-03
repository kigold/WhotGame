//using Microsoft.AspNetCore.SignalR;
//using WhotGame.Abstractions.Models;

//namespace WhoteGame.Services
//{
//    public interface IRealTimeClient
//    {
//        Task Test(long gameId, PlayerLite player);
//        Task StartGame(long gameId, PlayerLite player);
//        Task UpdateTurn(long gameId, PlayerLite player);
//    }

//    public class RealTimeClient : IRealTimeClient
//    {
//        private readonly IHubContext<Hub> _hubContext;
//        public RealTimeClient(IHubContext<Hub> hubContext) 
//        {
//            _hubContext = hubContext;
//        }

//        public async Task Test(long gameId, PlayerLite player)
//        {
//            await _hubContext.Clients.All.SendAsync("Test");
//        }

//        public async Task UpdateTurn(long gameId, PlayerLite player)
//        {
//            await _hubContext.Clients.Group(GenerateGameConnection(gameId)).SendAsync("UpdateTurn", player);
//        }

//        public async Task StartGame(long gameId, PlayerLite player)
//        {
//            await _hubContext.Clients.Group(GenerateGameConnection(gameId)).SendAsync("StartGame");
//        }

//        private string GeneratePlayerConnection(long gameId, long playerId)
//        {
//            return $"connect_player_{gameId}_{playerId}";
//        }

//        private string GenerateGameConnection(long gameId)
//        {
//            return $"connect_game{gameId}";
//        }
//    }
//}