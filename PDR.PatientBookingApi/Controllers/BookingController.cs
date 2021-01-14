using Microsoft.AspNetCore.Mvc;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices;
using PDR.PatientBooking.Service.BookingServices.Requests;
using System;
using System.Collections.Generic;
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

        private static MyOrderResult UpdateLatestBooking(List<Order> bookings2, int i)
        {
            return new MyOrderResult
            {
                Id = bookings2[i].Id,
                DoctorId = bookings2[i].DoctorId,
                StartTime = bookings2[i].StartTime,
                EndTime = bookings2[i].EndTime,
                PatientId = bookings2[i].PatientId,
                SurgeryType = (int)bookings2[i].GetSurgeryType()
            };
        }

        private class MyOrderResult
        {
            public Guid Id { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public long PatientId { get; set; }
            public long DoctorId { get; set; }
            public int SurgeryType { get; set; }
        }
    }
}