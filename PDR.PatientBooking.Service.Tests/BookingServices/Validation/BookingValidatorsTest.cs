using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Validation;
using System;

namespace PDR.PatientBooking.Service.Tests.BookingServices.Validation
{
    [TestFixture]
    class BookingValidatorsTest
    {
        private IFixture _fixture;

        private PatientBookingContext _context;

        private GetPatientNextAppointmentRequestValidation _getPatientNextAppointmentRequestValidator;
        private AddBookingRequestValidation _addBookingRequestValidation;
        private CancelBookingRequestValidation _cancelBookingRequestValidation;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _fixture = new Fixture();

            //Prevent fixture from generating from entity circular references 
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _cancelBookingRequestValidation = new CancelBookingRequestValidation(
                _context
            );

            _addBookingRequestValidation = new AddBookingRequestValidation(
                _context
            );

            _getPatientNextAppointmentRequestValidator = new GetPatientNextAppointmentRequestValidation(
                _context
            );
        }

        private void SetupMockDefaults()
        {

        }

        [Test]
        public void ValidateRequestGetPatientNextApp_AllChecksPass_ReturnsPassedValidationResult()
        {
            //arrange
            var order = _fixture.Create<Order>();
            order.Cancelled = false;
            order.StartTime = DateTime.UtcNow.AddDays(1);
            _context.Order.Add(order);
            _context.SaveChanges();

            var request = GetValidRequestGetPatientNextApp(order.PatientId);

            //act
            var res = _getPatientNextAppointmentRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeTrue();
        }

        [Test]
        public void ValidateRequestGetPatientNextApp_BookingNotFound_ReturnsFailedValidationResult()
        {
            //arrange
            var request = GetValidRequestGetPatientNextApp(100L);

            //act
            var res = _getPatientNextAppointmentRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("Active Bookings for Patient ID 100 not found");
        }

        [Test]
        public void ValidateRequestAddBooking_AllChecksPass_ReturnsPassedValidationResult()
        {
            //arrange
            var request = GetValidRequestAddBooking();

            //act
            var res = _addBookingRequestValidation.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeTrue();
        }

        [Test]
        public void ValidateRequestAddBooking_BookingIncorrectShift_ReturnsFailedValidationResult()
        {
            //arrange
            var request = GetValidRequestAddBooking();
            request.StartTime = DateTime.UtcNow;

            //act
            var res = _addBookingRequestValidation.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("Patients can't book appointments in the past.");
        }

        [Test]
        public void ValidateCancelBooking_BookingIncorrectPatientId_ReturnsFailedValidationResult()
        {
            //arrange
            var bookingId = Guid.NewGuid();

            var order = _fixture.Create<Order>();
            order.Cancelled = false;
            order.Id = bookingId;
            _context.Order.Add(order);
            _context.SaveChanges();

            var request = GetValidRequestCancelBooking(bookingId);

            //act
            var res = _cancelBookingRequestValidation.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain($"Booking with id: {bookingId} is not attached to Patient with id {request.PatientId}.");
        }

        private GetPatientNextAppointmentRequest GetValidRequestGetPatientNextApp(long patientId)
        {
            var request = _fixture.Build<GetPatientNextAppointmentRequest>()
                .With(x => x.PatientId, patientId)
                .Create();
            return request;
        }

        private AddBookingRequest GetValidRequestAddBooking()
        {
            var request = _fixture.Build<AddBookingRequest>()
                .With(x => x.StartTime, DateTime.UtcNow.AddDays(1))
                .With(x => x.EndTime, DateTime.UtcNow.AddDays(2))
                .Create();
            return request;
        }

        private CancelBookingRequest GetValidRequestCancelBooking(Guid bookingId)
        {
            var request = _fixture.Build<CancelBookingRequest>()
                .With(x => x.BookingId, bookingId)
                .Create();
            return request;
        }
    }
}
