using Microsoft.EntityFrameworkCore;
using Rezzy.Data;
using Rezzy.Models;
using Twilio;
using Twilio.AspNet.Common;
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;

namespace Rezzy.Controllers
{
    public class SmsController : TwilioController
    {
        private readonly RezzyContext _context;
        private readonly IConfiguration _configuration;

        public SmsController(RezzyContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<TwiMLResult> Index(SmsRequest incomingMessage)
        {
            var message = incomingMessage.Body.ToLowerInvariant();
            var phone = incomingMessage.From.Replace("+1", "");
            var reservation = await _context.Reservation.FirstOrDefaultAsync(r => r.Phone.Equals(phone));
            var response = new MessagingResponse();

            if (reservation == null)
            {
                response.Message(
                    "We're sorry but we're unable to locate your reservation. Please contact the restaurant at (503) 555-1212 for assistance");
                return TwiML(response);
            }

            switch (message)
            {
                case "yes":
                    return await ConfirmReservationAsync(reservation);
                    break;
                case "no":
                    return await CancelReservationAsync(reservation);
                    break;
                default:
                    return await UnkownSelectionAsync(reservation);
                    break;
            }
        }

        private async Task<TwiMLResult> ConfirmReservationAsync(Reservation reservation)
        {
            reservation.Status = ReservationStatus.Confirmed;
            await _context.SaveChangesAsync();
            var response = new MessagingResponse();
            response.Message(
                $"Thank you for confirming!  We look forward to serving you on {reservation.DateTime.ToShortDateString()} at {reservation.DateTime.ToShortTimeString()}.");
            return TwiML(response);
        }

        private async Task<TwiMLResult> CancelReservationAsync(Reservation reservation)
        {
            reservation.Status = ReservationStatus.Canceled;
            await _context.SaveChangesAsync();
            var response = new MessagingResponse();
            response.Message(
                $"Thank you for letting us know.  We look forward to serving you in the future.");
            return TwiML(response);
        }

        private async Task<TwiMLResult> UnkownSelectionAsync(Reservation reservation)
        {
            var response = new MessagingResponse();
            response.Message(
                $"We didn't understand what you said.  Please respond with YES or NO whether you will be dining with us on " +
                $"{reservation.DateTime.ToShortDateString()} at {reservation.DateTime.ToShortTimeString()}.");
            return TwiML(response);
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
