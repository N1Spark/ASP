using ASP.Data;
using Microsoft.EntityFrameworkCore;

namespace ASP.Services
{
    public class ExpiredTokenCleanService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceProvider _serviceProvider;

        public ExpiredTokenCleanService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var expiredTokens = context.Tokens
                                           .Where(t => t.ExpireDt < DateTime.Now)
                                           .ToList();

                if (expiredTokens.Any())
                {
                    context.Tokens.RemoveRange(expiredTokens);
                    context.SaveChanges();
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
