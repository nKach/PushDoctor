using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.Validation;
using System;
using PDR.PatientBooking.Service.BookingServices;
using PDR.PatientBooking.Service.BookingServices.Validation;
using PDR.PatientBooking.Service.BookingServices.Requests;
using System.Linq;
using FluentAssertions;
using PDR.PatientBooking.Data.Models;
using System.Data.Entity.Core;
using PDR.PatientBooking.Service.BookingServices.Responses;

namespace PDR.PatientBooking.Service.Tests.BookingServices
{
    [TestFixture]
    public class BookingServiceTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;

        private PatientBookingContext _context;
        private Mock<IGetPatientNextAppointmentRequestValidation> _getPatientNextAppointmentRequestValidator;
        private Mock<IAddBookingRequestValidation> _addBookingRequestValidation;
        private Mock<ICancelBookingRequestValidation> _cancelBookingRequestValidation;

        private BookingService _bookingService;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();

            //Prevent fixture from generating circular references
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            _getPatientNextAppointmentRequestValidator = _mockRepository.Create<IGetPatientNextAppointmentRequestValidation>();
            _addBookingRequestValidation = _mockRepository.Create<IAddBookingRequestValidation>();
            _cancelBookingRequestValidation = _mockRepository.Create<ICancelBookingRequestValidation>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _bookingService = new BookingService(
                _context,
                _getPatientNextAppointmentRequestValidator.Object,
                _addBookingRequestValidation.Object,
                _cancelBookingRequestValidation.Object
            );
        }

        private void SetupMockDefaults()
        {
            _getPatientNextAppointmentRequestValidator.Setup(x => x.ValidateRequest(It.IsAny<GetPatientNextAppointmentRequest>()))
                .Returns(new PdrValidationResult(true));

            _addBookingRequestValidation.Setup(x => x.ValidateRequest(It.IsAny<AddBookingRequest>()))
                .Returns(new PdrValidationResult(true));

            _cancelBookingRequestValidation.Setup(x => x.ValidateRequest(It.IsAny<CancelBookingRequest>()))
                .Returns(new PdrValidationResult(true));
        }

        [Test]
        public void AddBooking_ValidatesRequest()
        {
            //arrange
            var request = _fixture.Create<AddBookingRequest>();

            //act
            _bookingService.AddBooking(request);

            //assert
            _addBookingRequestValidation.Verify(x => x.ValidateRequest(request), Times.Once);
        }

        [Test]
        public void AddBooking_ValidatorFails_ThrowsArgumentException()
        {
            //arrange
            var failedValidationResult = new PdrValidationResult(false, _fixture.Create<string>());

            _addBookingRequestValidation.Setup(x => x.ValidateRequest(It.IsAny<AddBookingRequest>())).Returns(failedValidationResult);

            //act
            var exception = Assert.Throws<ArgumentException>(() => _bookingService.AddBooking(_fixture.Create<AddBookingRequest>()));

            //assert
            exception.Message.Should().Be(failedValidationResult.Errors.First());
        }

        [Test]
        public void AddBooking_Passed()
        {
            //arrange
            var request = _fixture.Create<AddBookingRequest>();

            var expected = new Order
            {
                Cancelled = false,
                Doctor = null,
                DoctorId = request.DoctorId,
                EndTime = request.EndTime,
                Patient = null,
                PatientId = request.PatientId,
                StartTime = request.StartTime,
                SurgeryType = 0
            };

            //act
            _bookingService.AddBooking(request);

            //assert
            _context.Order.Should().ContainEquivalentOf(expected, options => options.Excluding(order => order.Id));
        }

        [Test]
        public void GetPatientNextAppointment_ValidatesRequest()
        {
            //arrange
            var request = _fixture.Create<GetPatientNextAppointmentRequest>();

            //act
            _bookingService.GetPatientNextAppointment(request);

            //assert
            _getPatientNextAppointmentRequestValidator.Verify(x => x.ValidateRequest(request), Times.Once);
        }

        [Test]
        public void GetPatientNextAppointment_ValidatorFails_ThrowsObjectNotFoundException()
        {
            //arrange
            var failedValidationResult = new PdrValidationResult(false, _fixture.Create<string>());

            _getPatientNextAppointmentRequestValidator.Setup(x => x.ValidateRequest(It.IsAny<GetPatientNextAppointmentRequest>())).Returns(failedValidationResult);

            //act
            var exception = Assert.Throws<ObjectNotFoundException>(() => _bookingService.GetPatientNextAppointment(_fixture.Create<GetPatientNextAppointmentRequest>()));

            //assert
            exception.Message.Should().Be(failedValidationResult.Errors.First());
        }

        [Test]
        public void GetPatientNextAppointment_Passed()
        {
            //arrange
            var order = _fixture.Create<Order>();
            order.Cancelled = false;
            order.StartTime = DateTime.UtcNow.AddDays(1);
            _context.Order.Add(order);
            _context.SaveChanges();

            var expected = new GetPatientNextAppointmentResponse
            {
                StartTime = order.StartTime,
                DoctorId = order.DoctorId,
                EndTime = order.EndTime,
                Id = order.Id
            };

            //act
            var res = _bookingService.GetPatientNextAppointment(new GetPatientNextAppointmentRequest
            {
                PatientId = order.PatientId
            });

            //assert
            res.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void CancelBooking_ValidatesRequest()
        {
            //arrange
            var request = _fixture.Create<CancelBookingRequest>();

            //act
            _bookingService.CancelBooking(request);

            //assert
            _cancelBookingRequestValidation.Verify(x => x.ValidateRequest(request), Times.Once);
        }

        [Test]
        public void CancelBooking_ValidatorFails_ThrowsArgumentException()
        {
            //arrange
            var failedValidationResult = new PdrValidationResult(false, _fixture.Create<string>());

            _cancelBookingRequestValidation.Setup(x => x.ValidateRequest(It.IsAny<CancelBookingRequest>())).Returns(failedValidationResult);

            //act
            var exception = Assert.Throws<ArgumentException>(() => _bookingService.CancelBooking(_fixture.Create<CancelBookingRequest>()));

            //assert
            exception.Message.Should().Be(failedValidationResult.Errors.First());
        }

        [Test]
        public void CancelBooking_Passed()
        {
            //arrange
            var order = _fixture.Create<Order>();
            order.Cancelled = false;
            order.StartTime = DateTime.UtcNow.AddDays(1);
            _context.Order.Add(order);
            _context.SaveChanges();

            //act
            _bookingService.CancelBooking(new CancelBookingRequest()
            {
                BookingId = order.Id,
                PatientId = order.PatientId
            });

            //assert
            order.Cancelled.Should().BeTrue();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }
    }
}
