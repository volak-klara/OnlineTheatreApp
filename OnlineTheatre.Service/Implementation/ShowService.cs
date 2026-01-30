using Microsoft.EntityFrameworkCore;
using OnlineTheatre.Domain.DomainModels;
using OnlineTheatre.Repository.Implementation;
using OnlineTheatre.Repository.Interface;
using OnlineTheatre.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineTheatre.Service.Implementation
{
    public class ShowService : IShowService
    {
        private readonly IRepository<Show> _showRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly ISeatGeekService seatGeekService;

        public ShowService(IRepository<Show> showRepository, IRepository<Ticket> ticketRepository, ISeatGeekService seatGeekService)
        {
            _showRepository = showRepository;
            _ticketRepository = ticketRepository;
            this.seatGeekService = seatGeekService;
        }

        public List<Show> GetAll()
        {
            return _showRepository.GetAll(
                selector: x => x,
                predicate: x => x.StartTime >= DateTime.UtcNow.Date,
                include: x => x.Include(s => s.Tickets)
            ).ToList();
        }

        public Show? GetById(Guid id)
        {
            return _showRepository.Get(
                selector: x => x,
                predicate: x => x.Id == id,
                include: x => x.Include(s => s.Tickets)
            );
        }

        public Show Insert(Show show)
        {

            if (show.Id == Guid.Empty)
                show.Id = Guid.NewGuid();


            _showRepository.Insert(show);


            GenerateTicketsForShow(show.Id, show.BasePrice);

            return _showRepository.Get(
                selector: s => s,
                predicate: s => s.Id == show.Id,
                include: q => q.Include(s => s.Tickets)
            )!;
        }

        public Show Update(Show show)
        {
            var existingShow = GetById(show.Id);
            if (existingShow == null)
                throw new Exception("Show not found");


            if (existingShow.BasePrice != show.BasePrice)
            {
                foreach (var ticket in existingShow.Tickets!)
                {
                    ticket.Price = show.BasePrice;
                    _ticketRepository.Update(ticket);
                }
            }

            return _showRepository.Update(show);
        }

        public Show DeleteById(Guid id)
        {
            var show = GetById(id);
            if (show == null)
                throw new Exception("Show not found");

            return _showRepository.Delete(show);
        }


        private void GenerateTicketsForShow(Guid showId, decimal price)
        {
            var tickets = new List<Ticket>();


            for (int row = 1; row <= 5; row++)
            {
                for (int seat = 1; seat <= 4; seat++)
                {
                    tickets.Add(new Ticket
                    {
                        Id = Guid.NewGuid(),
                        ShowId = showId,
                        SeatLabel = $"A{row}-{seat}",
                        Price = price,
                        IsSold = false
                    });
                }
            }


            for (int row = 1; row <= 5; row++)
            {
                for (int seat = 1; seat <= 4; seat++)
                {
                    tickets.Add(new Ticket
                    {
                        Id = Guid.NewGuid(),
                        ShowId = showId,
                        SeatLabel = $"B{row}-{seat}",
                        Price = price,
                        IsSold = false
                    });
                }
            }


            for (int seat = 1; seat <= 10; seat++)
            {
                tickets.Add(new Ticket
                {
                    Id = Guid.NewGuid(),
                    ShowId = showId,
                    SeatLabel = $"C1-{seat}",
                    Price = price,
                    IsSold = false
                });
            }

            _ticketRepository.InsertMany(tickets);
        }


        public bool ExistsByExternalId(string externalId)
        {
            return _showRepository.Get(
                selector: x => x,
                predicate: x => x.ExternalId == externalId
            ) != null;
        }



        //public async Task<int> ImportFromSeatGeekAsync(int count = 10)
        //{
        //    var data = await seatGeekService.GetTheatreEventsAsync(page: 1, perPage: 50);
        //    if (data?.Events == null || data.Events.Count == 0)
        //        return 0;

        //    // земи први N валидни настани
        //    var events = data.Events
        //        .Where(e => !string.IsNullOrWhiteSpace(e.Title))
        //        .Take(count)
        //        .ToList();

        //    if (!events.Any())
        //        return 0;

        //    var imported = 0;

        //    // 🔹 најди последна претстава во база
        //    var lastShowDate = _showRepository
        //        .GetAll(selector: x => x.StartTime)
        //        .OrderByDescending(d => d)
        //        .FirstOrDefault();

        //    // 🔹 стартен датум
        //    DateTime currentDate = lastShowDate == DateTime.MinValue
        //        ? DateTime.Today.AddHours(20)
        //        : lastShowDate.Date.AddDays(1).AddHours(20);

        //    // 🔹 креирај претстави една по една, по ред
        //    foreach (var ev in events)
        //    {
        //        var image = ev.Performers?
        //            .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Image))
        //            ?.Image;

        //        var show = new Show
        //        {
        //            Id = Guid.NewGuid(),
        //            Titile = ev.Title,
        //            StartTime = currentDate,
        //            BasePrice = 250,
        //            DurationMinutes = 120,
        //            ImageUrl = image
        //        };

        //        _showRepository.Insert(show);               // SaveChanges
        //        GenerateTicketsForShow(show.Id, show.BasePrice);

        //        imported++;

        //        // следната претстава е наредниот ден во 20:00
        //        currentDate = currentDate.AddDays(1);
        //    }

        //    return imported;
        //}

        public async Task<int> ImportFromSeatGeekAsync(int count = 10)
        {
            var data = await seatGeekService.GetTheatreEventsAsync(page: 1, perPage: 50);
            if (data?.Events == null || data.Events.Count == 0)
                return 0;

            var events = data.Events
                .Where(e => !string.IsNullOrWhiteSpace(e.Title))
                .Take(count)
                .ToList();

            if (!events.Any())
                return 0;

            var imported = 0;

            // 🔹 најди последна претстава
            var lastShowDate = _showRepository
                .GetAll(selector: x => x.StartTime)
                .OrderByDescending(d => d)
                .FirstOrDefault();

            // 🔹 стартен датум (UTC!)
            DateTime currentDate;

            if (lastShowDate == DateTime.MinValue)
            {
                currentDate = DateTime.UtcNow.Date.AddHours(20);
            }
            else
            {
                currentDate = lastShowDate
                    .ToUniversalTime()
                    .Date
                    .AddDays(1)
                    .AddHours(20);
            }

            foreach (var ev in events)
            {
                var image = ev.Performers?
                    .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Image))
                    ?.Image;

                var show = new Show
                {
                    Id = Guid.NewGuid(),
                    Titile = ev.Title,
                    StartTime = DateTime.SpecifyKind(currentDate, DateTimeKind.Utc),
                    BasePrice = 250,
                    DurationMinutes = 120,
                    ImageUrl = image
                };

                _showRepository.Insert(show);
                GenerateTicketsForShow(show.Id, show.BasePrice);

                imported++;

                // следната вечер во 20:00 UTC
                currentDate = currentDate.AddDays(1);
            }

            return imported;
        }



    }
}