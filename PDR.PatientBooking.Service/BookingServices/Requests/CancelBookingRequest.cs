using System;

namespace PDR.PatientBooking.Service.BookingServices.Requests
{
    public class CancelBookingRequest
    {
        public Guid BookingId { get; set; }
        public long PatientId { get; set; }
    }
}
