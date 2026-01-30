using Microsoft.EntityFrameworkCore;
using OnlineTheatre.Domain.DomainModels;
using OnlineTheatre.Domain.DTO;
using OnlineTheatre.Repository.Interface;
using OnlineTheatre.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using QRCoder;

namespace OnlineTheatre.Service.Implementation
{

    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepository;

        public OrderService(IRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        private static string ToQrPngBase64(string payload)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            using var qr = new PngByteQRCode(data);
            var bytes = qr.GetGraphic(8);
            return Convert.ToBase64String(bytes);
        }
        public List<PurchasedTicketDTO> GetActiveTickets(string userId)
        {
            var now = DateTime.Now;

           
            var orders = _orderRepository.GetAll(
                selector: o => o,
                predicate: o => o.OwnerId == userId,
                include: q => q
                    .Include(o => o.TicketsInOrder)
                        .ThenInclude(tio => tio.OrderedTicket)
                            .ThenInclude(t => t.Show)
            ).ToList();

            
            var tickets = orders
                .SelectMany(o => o.TicketsInOrder ?? new List<TicketInOrder>(),
                    (o, tio) => new PurchasedTicketDTO
                    {
                        OrderId = o.Id,
                        TicketId = tio.TicketId,
                        ShowTitle = tio.OrderedTicket?.Show?.Titile ?? "(Unknown show)",
                        StartTime = tio.OrderedTicket?.Show?.StartTime ?? DateTime.MinValue,
                        DurationMinutes = tio.OrderedTicket?.Show?.DurationMinutes ?? 0,
                        SeatLabel = tio.OrderedTicket?.SeatLabel ?? "",
                        Price = tio.OrderedTicket?.Price ?? 0,

                        QrPngBase64 = string.IsNullOrEmpty(tio.QrPayload) ? "" : ToQrPngBase64(tio.QrPayload)
                    })
               
                .Where(x => x.StartTime != DateTime.MinValue)
                .Where(x => x.StartTime.AddMinutes(x.DurationMinutes) > now)
                .OrderBy(x => x.StartTime)
                .ThenBy(x => x.SeatLabel)
                .ToList();

            return tickets;
        }

        public List<Order> GetAllOrders()
        {
            return _orderRepository.GetAll(
                selector: x => x,
                include: x => x.Include(o => o.TicketsInOrder)
                               .ThenInclude(tio => tio.OrderedTicket)
                               .Include(o => o.Owner)
            ).ToList();
        }

        public Order? GetOrder(Guid id)
        {
            return _orderRepository.Get(
                selector: x => x,
                predicate: x => x.Id == id,
                include: x => x.Include(o => o.TicketsInOrder)
                               .ThenInclude(tio => tio.OrderedTicket)
                               .Include(o => o.Owner)
            );
        }

        public List<PurchasedTicketDTO> GetPastTickets(string userId)
        {
            var now = DateTime.Now;

            var orders = _orderRepository.GetAll(
                selector: o => o,
                predicate: o => o.OwnerId == userId,
                include: q => q
                    .Include(o => o.TicketsInOrder)
                        .ThenInclude(tio => tio.OrderedTicket)
                            .ThenInclude(t => t.Show)
            ).ToList();

            var tickets = orders
                .SelectMany(o => o.TicketsInOrder ?? new List<TicketInOrder>(),
                    (o, tio) => new PurchasedTicketDTO
                    {
                        OrderId = o.Id,
                        TicketId = tio.TicketId,
                        ShowTitle = tio.OrderedTicket?.Show?.Titile ?? "(Unknown show)",
                        StartTime = tio.OrderedTicket?.Show?.StartTime ?? DateTime.MinValue,
                        DurationMinutes = tio.OrderedTicket?.Show?.DurationMinutes ?? 0,
                        SeatLabel = tio.OrderedTicket?.SeatLabel ?? "",
                        Price = tio.OrderedTicket?.Price ?? 0
                    })
               
                .Where(x => x.StartTime != DateTime.MinValue)
                .Where(x => x.StartTime.AddMinutes(x.DurationMinutes) <= now)
                .OrderByDescending(x => x.StartTime)
                .ThenBy(x => x.SeatLabel)
                .ToList();

            return tickets;
        }
    }
}
