using HotelBookingSystem.Data;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using BookingService.Entities;



namespace HotelBookingSystem.Services
{
    public class NotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _queueName;

        public NotificationService(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            var connectionString = configuration.GetSection("ServiceBus")["ConnectionString"];
            _queueName = configuration.GetSection("ServiceBus")["QueueName"];
            _serviceBusClient = new ServiceBusClient(connectionString);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Service Bus processor başlat
            var processor = _serviceBusClient.CreateProcessor(_queueName, new ServiceBusProcessorOptions());

            processor.ProcessMessageAsync += async args =>
            {
                var body = args.Message.Body.ToString();

                Console.WriteLine($"[ServiceBus] Received message: {body}");

                // Booking deserialize
                var booking = System.Text.Json.JsonSerializer.Deserialize<Booking>(body);

                // Admin notification simülasyonu
                Console.WriteLine($"[Notification] New Reservation: BookingId {booking.BookingId}, HotelId {booking.HotelId}, UserId {booking.UserId}");

                await args.CompleteMessageAsync(args.Message);
            };

            processor.ProcessErrorAsync += args =>
            {
                Console.WriteLine($"[ServiceBus] ERROR: {args.Exception.Message}");
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync(stoppingToken);

            // CheckRoomCapacities döngüsü devam ediyor
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                if (now.Hour == 2 && now.Minute == 0)
                {
                    await CheckRoomCapacities();
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task CheckRoomCapacities()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();

                var rooms = db.Rooms.ToList();

                foreach (var room in rooms)
                {
                    if (room.TotalCount == 0) continue;

                    var ratio = (double)room.AvailableCount / room.TotalCount;

                    if (ratio < 0.20)
                    {
                        Console.WriteLine($"[Notification] ROOM ALERT: HotelId: {room.HotelId} RoomId: {room.RoomId} capacity below 20%.");
                        // Gerçek projede: mail, SMS veya API çağrısı yaparsın
                    }
                }
            }

            await Task.CompletedTask;
        }
    }
}
