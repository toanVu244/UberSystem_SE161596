using UberSystem.Service;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using UberSystem.Domain.Interfaces.Services;

namespace UberSystem.Api.Authentication.Controllers
{
    public class DriverDataSenderService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceProvider _serviceProvider;
        private long _userId;

        public DriverDataSenderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        //public void ReceiveUserId(long userId)
        //{
        //    Console.WriteLine($"Received userId: {userId}");
        //    Interlocked.Exchange(ref _userId, userId); // Cập nhật an toàn
        //    Console.WriteLine($"Updated _userId: {_userId}");
        //    if (_userId != 0 && _timer == null)
        //    {
        //        _timer = new Timer(SendData, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
        //        Console.WriteLine("Timer started.");
        //    }
        //}

        //public Task StartAsync(CancellationToken cancellationToken)
        //{
        //    Console.WriteLine("Starting DriverDataSenderService...");
        //    _timer = new Timer(SendData, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
        //    return Task.CompletedTask;
        //}
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Starting DriverDataSenderService...");
            return Task.CompletedTask; // Không khởi tạo Timer ở đây
        }
        public void ReceiveUserId(long userId)
        {
            Console.WriteLine($"Received userId: {userId}");
            Interlocked.Exchange(ref _userId, userId); // Cập nhật an toàn
            Console.WriteLine($"Updated _userId: {_userId}");

            // Bắt đầu Timer nếu _userId hợp lệ
            if (_userId != 0 && _timer == null)
            {
                _timer = new Timer(SendData, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
                Console.WriteLine("Timer started.");
            }
            // Dừng Timer nếu _userId không hợp lệ
            else if (_userId == 0 && _timer != null)
            {
                _timer.Change(Timeout.Infinite, 0);
                _timer.Dispose();
                _timer = null;
                Console.WriteLine("Timer stopped.");
            }
        }


        private async void SendData(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var driverService = scope.ServiceProvider.GetRequiredService<IDriverService>();

                try
                {
                    Console.WriteLine("Sending driver data...");
                    await Task.Delay(100); // Thêm delay 100ms
                    Console.WriteLine($"Current _userId in SendData: {_userId}");

                    if (_userId != 0)
                    {
                        await driverService.GetDriverRequestAsync(_userId);
                    }
                    else
                    {
                        Console.WriteLine("UserId is not set. Skipping data send.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in SendData: {ex.Message}");
                }
            }
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Stopping DriverDataSenderService...");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Console.WriteLine("Disposing DriverDataSenderService...");
            _timer?.Dispose();
        }
    }
}
