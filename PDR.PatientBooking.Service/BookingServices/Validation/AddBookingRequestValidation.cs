using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class AddBookingRequestValidation : IAddBookingRequestValidation
    {
        private readonly PatientBookingContext _context;

        public AddBookingRequestValidation(PatientBookingContext context)
        {
            _context = context;
        }

        public PdrValidationResult ValidateRequest(AddBookingRequest request)
        {
            var result = new PdrValidationResult(true);

            if (BookingIncorrectShift(request, ref result))
                return result;

            if (BookingExpired(request, ref result))
                return result;

            if (DoubleBooking(request, ref result))
                return result;

            return result;
        }

        private bool BookingIncorrectShift(AddBookingRequest request, ref PdrValidationResult result)
        {
            var errors = new List<string>();

            if (request.StartTime >= request.EndTime)
                errors.Add("Booking End time can't be less or equal than booking Start time.");

            if (errors.Any())
            {
                result.PassedValidation = false;
                result.Errors.AddRange(errors);
                return true;
            }

            return false;
        }

        private bool BookingExpired(AddBookingRequest request, ref PdrValidationResult result)
        {
            var errors = new List<string>();

            if (request.StartTime < DateTime.UtcNow)
                errors.Add("Patients can't book appointments in the past.");

            if (errors.Any())
            {
                result.PassedValidation = false;
                result.Errors.AddRange(errors);
                return true;
            }

            return false;
        }

        private bool DoubleBooking(AddBookingRequest request, ref PdrValidationResult result)
        {
            var errors = new List<string>();

            var bookings = _context.Order
                .Where(x => x.DoctorId == request.DoctorId && x.StartTime > DateTime.UtcNow)
                .ToList();

            foreach (var booking in bookings)
            {
                if ((request.StartTime >= booking.StartTime && request.EndTime <= booking.EndTime) ||
                    (request.EndTime >= booking.StartTime && request.EndTime <= booking.EndTime))
                {
                    errors.Add($"Doctor {booking.Doctor.FirstName} {booking.Doctor.LastName} can't have two booking at the same time.");
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
