using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Validation;
using System.Collections.Generic;
using System.Linq;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class CancelBookingRequestValidation : ICancelBookingRequestValidation
    {
        private readonly PatientBookingContext _context;

        public CancelBookingRequestValidation(PatientBookingContext context)
        {
            _context = context;
        }

        public PdrValidationResult ValidateRequest(CancelBookingRequest request)
        {
            var result = new PdrValidationResult(true);

            if (IncorrectData(request, ref result))
                return result;

            return result;
        }

        private bool IncorrectData(CancelBookingRequest request, ref PdrValidationResult result)
        {
            var errors = new List<string>();

            var booking = _context.Order
                .Where(x => x.Id == request.BookingId)
                .SingleOrDefault();

            if (booking == null)
            {
                errors.Add($"Booking with id {request.BookingId} does not exist.");
            }
            else
            {
                if (booking.PatientId != request.PatientId)
                {
                    errors.Add($"Booking with id: {request.BookingId} is not attached to Patient " +
                        $"with id {request.PatientId}.");
                }

                if (booking.Cancelled)
                {
                    errors.Add($"Booking with id: {request.BookingId} already cancelled.");
                }
            }

            if (errors.Any())
            {
                result.PassedValidation = false;
                result.Errors.AddRange(errors);
                return true;
            }

            return false;
        }
    }
}
