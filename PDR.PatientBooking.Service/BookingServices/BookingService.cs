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

        private readonly IGetPatientNextAppointmentRequestValidation _validator;

        public BookingService(PatientBookingContext context, IGetPatientNextAppointmentRequestValidation validator)
        {
            _context = context;
            _validator = validator;
        }

        public GetPatientNextAppointmentResponse GetPatientNextAppointment(GetPatientNextAppointmentRequest request)
        {
            var validationResult = _validator.ValidateRequest(request);

            if (!validationResult.PassedValidation)
            {
                throw new ObjectNotFoundException(validationResult.Errors.First());
            }

            var bookings = _context.Order
                .Where(x => x.Patient.Id == request.PatientId && x.StartTime > DateTime.Now)
                .OrderBy(x => x.StartTime)
                .ToList();

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
                if (Enum.IsDefined(typeof(SurgeryType), bookingPatient.Clinic?.SurgeryType))
                {
                    booking.SurgeryType = (int)bookingPatient.Clinic.SurgeryType;
                }
            }

            _context.Order.AddRange(new List<Order> { booking });

            _context.SaveChanges();
        }
    }
}
