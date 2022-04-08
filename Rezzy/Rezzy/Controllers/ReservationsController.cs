#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rezzy.Data;
using Rezzy.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Rezzy.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly RezzyContext _context;
        private readonly IConfiguration _configuration;

        public ReservationsController(RezzyContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            return View(await _context.Reservation.OrderBy(r => r.DateTime).ToListAsync());
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservation
                .FirstOrDefaultAsync(m => m.ReservationID == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReservationID,FirstName,LastName,DateTime,Email,Phone,PartySize,Comments")] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reservation);
                await _context.SaveChangesAsync();
                await SendNewBookingConfirmation(reservation);
                return RedirectToAction(nameof(Index));
            }

            return View(reservation);
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservation.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReservationID,FirstName,LastName,DateTime,Email,Phone,PartySize,Comments")] Reservation reservation)
        {
            if (id != reservation.ReservationID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                    await SendRescheduleConfirmation(reservation);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.ReservationID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(reservation);
        }

        [HttpGet]
        public async Task<IActionResult> SendConfirmation(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var reservation = await _context.Reservation
                .FirstOrDefaultAsync(m => m.ReservationID == id);
            if (reservation == null)
            {
                return NotFound();
            }

            await SendConfirmationRequest(reservation);
            return View(null);
        }

        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var reservation = await _context.Reservation
                .FirstOrDefaultAsync(m => m.ReservationID == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservation.FindAsync(id);
            var deleted = reservation;
            _context.Reservation.Remove(deleted);
            await _context.SaveChangesAsync();
            await SendCancelationConfirmation(reservation);
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservation.Any(e => e.ReservationID == id);
        }

        private async Task SendNewBookingConfirmation(Reservation reservation)
        {
            var message = $"Hello {reservation.FirstName}! This is Morton's of Chicago - Portland. This message confirms your reservation on " +
                          $"{reservation.DateTime.ToShortDateString()} at {reservation.DateTime.ToShortTimeString()}. " +
                          $"We look forward to serving you!";
            await SendMessage(reservation, message);
        }

        private async Task SendConfirmationRequest(Reservation reservation)
        {
            var message =
                $"Hello {reservation.FirstName}! This is Morton's of Chicago - Portland. We are looking forward to serving you on {reservation.DateTime.ToShortDateString()} at {reservation.DateTime.ToShortTimeString()}. " +
                $"Please respond with YES to confirm or NO to cancel your reservation.";

            await SendMessage(reservation, message);
        }

        private async Task SendCancelationConfirmation(Reservation reservation)
        {
            var message =
                $"Hello {reservation.FirstName}! This confirms your reservation cancellation. We look forward to serving you in the future.";
            await SendMessage(reservation, message);
        }

        private async Task SendRescheduleConfirmation(Reservation reservation)
        {
            var message = $"Hello {reservation.FirstName}! This is Morton's of Chicago - Portland. This message confirms your new rescheduled reservation on " +
                          $"{reservation.DateTime.ToShortDateString()} at {reservation.DateTime.ToShortTimeString()}. " +
                          $"We look forward to serving you!";

            await SendMessage(reservation, message);
        }

        private async Task SendMessage(Reservation reservation, string messageText)
        {
            var accountSID = _configuration["TwilioConfig:AccountSID"];
            var token = _configuration["TwilioConfig:AuthToken"];
            var fromPhone = new Twilio.Types.PhoneNumber(_configuration["TwilioConfig:FromPhone"]);
            var toPhone = new Twilio.Types.PhoneNumber($"+1{reservation.Phone}");

            TwilioClient.Init(accountSID, token);
            await MessageResource.CreateAsync(body: messageText, from: fromPhone, to: toPhone);
        }
    }
}
