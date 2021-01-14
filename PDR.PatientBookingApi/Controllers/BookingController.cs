using Microsoft.AspNetCore.Mvc;
using PDR.PatientBooking.Service.BookingServices;
using PDR.PatientBooking.Service.BookingServices.Requests;
using System;
using System.Data.Entity.Core;

namespace PDR.PatientBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("patient/{patientId}/next")]
        public IActionResult GetPatientNextAppointment(long patientId)
        {
            try
            {
                return Ok(_bookingService.GetPatientNextAppointment(new GetPatientNextAppointmentRequest
                {
                    PatientId = patientId
                }));
            }
            catch (ObjectNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpPost()]
        public IActionResult AddBooking(AddBookingRequest request)
        {
            try
            {
                _bookingService.AddBooking(request);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpPut()]
        public IActionResult CancelBooking(CancelBookingRequest request)
        {
            try
            {
                _bookingService.CancelBooking(request);
                return Ok();
            }
            catch (ObjectNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
    }
}