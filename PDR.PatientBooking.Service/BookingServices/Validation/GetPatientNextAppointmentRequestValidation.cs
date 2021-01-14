using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Validation;
using System.Collections.Generic;
using System.Linq;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class GetPatientNextAppointmentRequestValidation : IGetPatientNextAppointmentRequestValidation
    {
        private readonly PatientBookingContext _context;

        public GetPatientNextAppointmentRequestValidation(PatientBookingContext context)
        {
            _context = context;
        }

        public PdrValidationResult ValidateRequest(GetPatientNextAppointmentRequest request)
        {
            var result = new PdrValidationResult(true);

            if (BookingNotFound(request, ref result))
                return result;

            return result;
        }

        private bool BookingNotFound(GetPatientNextAppointmentRequest request, ref PdrValidationResult result)
        {
            var errors = new List<string>();

            var isBookingsExist = _context.Order
                .Where(x => x.PatientId == request.PatientId && !x.Cancelled)
                .Any();

            if (!isBookingsExist)
                errors.Add($"Active Bookings for Patient ID {request.PatientId} not found");

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
