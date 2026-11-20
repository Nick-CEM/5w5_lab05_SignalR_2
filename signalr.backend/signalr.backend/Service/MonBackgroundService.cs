using Microsoft.AspNetCore.SignalR;
using signalr.backend.Data;
using signalr.backend.Hubs;
using signalr.backend.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace signalr.backend.Service
{
    public class MonBackgroundService : BackgroundService
    {
        // Delai de 30 sec
        public const int DELAY = 30 * 1000;

        private IHubContext<ChatHub> _chatHub;
        private IServiceScopeFactory _serviceScopeFactory;

        public MonBackgroundService(IHubContext<ChatHub> monHub, IServiceScopeFactory serviceScopeFactory) {
            _chatHub = monHub;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task CheckMostPopularChannel(CancellationToken stoppingToken) {
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // On peut maintenant utiliser le dbContext normalement
                // On peut également faire un SaveChanges

                // je prend le channel avec le plus de nbMessages et ensuite je prend son nb de messages
                Channel channelWithMostMessages = dbContext.Channel.OrderByDescending(c => c.NbMessages).First();
                int nbMessages = channelWithMostMessages.NbMessages;

                await _chatHub.Clients.Group("Channel" + channelWithMostMessages.Id).SendAsync("MostPopularChannel", nbMessages, stoppingToken);
            }
            // Une fois que l'on va sortir du "using", le scope va être détruit et le dbContext associé au scope va également être détruit!
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(DELAY, stoppingToken);
                await CheckMostPopularChannel(stoppingToken);
            }
        }
    }
}
