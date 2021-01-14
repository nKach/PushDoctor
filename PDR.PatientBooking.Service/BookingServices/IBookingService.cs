using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;

namespace PDR.PatientBooking.Service.BookingServices
{
    public interface IBookingService
    {
        GetPatientNextAppointmentResponse GetPatientNextAppointment(GetPatientNextAppointmentRequest request);
        void AddBooking(AddBookingRequest request);
        void CancelBooking(CancelBookingRequest request);
    }
}
