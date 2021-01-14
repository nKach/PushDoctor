using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;
using PDR.PatientBooking.Service.BookingServices.Validation;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;

namespace PDR.PatientBooking.Service.BookingServices
{
    public class BookingService : IBookingService
    {
        private readonly PatientBookingContext _context;
        private readonly IGetPatientNextAppointmentRequestValidation _getPatientNextAppointmentRequestValidator;
        private readonly IAddBookingRequestValidation _addBookingRequestValidation;
        private readonly ICancelBookingRequestValidation _cancelBookingRequestValidation;

        public BookingService(PatientBookingContext context,
            IGetPatientNextAppointmentRequestValidation getPatientNextAppointmenRequesttValidator,
            IAddBookingRequestValidation addBookingRequestValidation,
            ICancelBookingRequestValidation cancelBookingRequestValidation)
        {
            _context = context;
            _getPatientNextAppointmentRequestValidator = getPatientNextAppointmenRequesttValidator;
            _addBookingRequestValidation = addBookingRequestValidation;
            _cancelBookingRequestValidation = cancelBookingRequestValidation;
        }

        public GetPatientNextAppointmentResponse GetPatientNextAppointment(GetPatientNextAppointmentRequest request)
        {
            var validationResult = _getPatientNextAppointmentRequestValidator.ValidateRequest(request);

            if (!validationResult.PassedValidation)
            {
                throw new ObjectNotFoundException(validationResult.Errors.First());
            }

            var bookings = _context.Order
                .Where(x => x.PatientId == request.PatientId && x.StartTime > DateTime.UtcNow && !x.Cancelled)
                .OrderBy(x => x.StartTime)
                .ToList();

            if (!bookings.Any())
            {
                return new GetPatientNextAppointmentResponse();
            }

            return new GetPatientNextAppointmentResponse
            {
                Id = bookings.First().Id,
                DoctorId = bookings.First().DoctorId,
                StartTime = bookings.First().StartTime,
                EndTime = bookings.First().EndTime
            };
        }

        public void AddBooking(AddBookingRequest request)
        {
            var validationResult = _addBookingRequestValidation.ValidateRequest(request);

            if (!validationResult.PassedValidation)
            {
                throw new ArgumentException(validationResult.Errors.First());
            }

            var bookingPatient = _context.Patient.FirstOrDefault(x => x.Id == request.PatientId);

            var bookingDoctor = _context.Doctor.FirstOrDefault(x => x.Id == request.DoctorId);

            var booking = new Order
            {
                Id = new Guid(),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                Patient = bookingPatient,
                Doctor = bookingDoctor,
            };

            if (bookingPatient?.Clinic != null)
            {
                booking.SurgeryType = (int)bookingPatient.Clinic.SurgeryType;
            }

            _context.Order.AddRange(new List<Order> { booking });

            _context.SaveChanges();
        }

        public void CancelBooking(CancelBookingRequest request)
        {
            var validationResult = _cancelBookingRequestValidation.ValidateRequest(request);

            if (!validationResult.PassedValidation)
            {
                if (validationResult.Errors.Contains($"Booking with id {request.BookingId} does not exist."))
                {
                    throw new ObjectNotFoundException($"Booking with id {request.BookingId} does not exist.");
                }

                throw new ArgumentException(validationResult.Errors.First());
            }

            var booking = _context.Order.FirstOrDefault(x => x.Id == request.BookingId && !x.Cancelled);

            if (booking != null)
            {
                booking.Cancelled = true;

                _context.Order.Update(booking);

                _context.SaveChanges();
            }
        }
    }
}
