using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Interfaces;
using UberSystem.Domain.Repository;
using UberSystem.Domain.Request;
using UberSystem.Infrastructure.Repository;

namespace UberSystem.Service
{
    public interface ICustomerService
    {
        Task<string> Booking(BookingRequest bookingRequest, long? tripId, float? amount, string? method, string? status);
        Task<string> GetStatusTrip(long tripId, long customerId, int? rating, string? feedback);
    }
    public class CustomerService : ICustomerService
    {
        private readonly IGSPRepository gSPRepository;
        private readonly ICustomerRepository customerRepository;
        private readonly IDriverRepository driverRepository;
        private readonly ITripRepository tripRepository;
        private readonly IPaymentRepository paymentRepository;
        private readonly IRatingRepository ratingRepository;
        private readonly IUserRepository userRepository;

        public CustomerService(ICustomerRepository customerRepository, IGSPRepository gSPRepository, IDriverRepository driverRepository, ITripRepository tripRepository, IPaymentRepository paymentRepository, IRatingRepository ratingRepository, IUserRepository userRepository)
        {
            this.customerRepository = customerRepository;
            this.gSPRepository = gSPRepository;
            this.driverRepository = driverRepository;
            this.tripRepository = tripRepository;
            this.paymentRepository = paymentRepository;
            this.ratingRepository = ratingRepository;  
            this.userRepository = userRepository;
        }

        private double CalculateDistance(double latitudePStart, double longitudePStart, double latitudeEnd, double longitudeEnd)
        {
            // Bán kính trái đất (km)
            const double EarthRadius = 6371.0;

            // Chuyển đổi độ sang radian
            double latPStartRad = ToRadians(latitudePStart);
            double lonPStartRad = ToRadians(longitudePStart);
            double latEndRad = ToRadians(latitudeEnd);
            double lonEndRad = ToRadians(longitudeEnd);

            // Tính sự khác biệt về kinh độ và vĩ độ
            double deltaLat = latEndRad - latPStartRad;
            double deltaLon = lonEndRad - lonPStartRad;

            // Áp dụng công thức Haversine
            double a = Math.Pow(Math.Sin(deltaLat / 2), 2) +
                       Math.Cos(latPStartRad) * Math.Cos(latEndRad) *
                       Math.Pow(Math.Sin(deltaLon / 2), 2);

            double c = 2 * Math.Asin(Math.Sqrt(a));

            // Tính khoảng cách
            return EarthRadius * c; // Khoảng cách tính bằng km
        }

        // Hàm chuyển đổi từ độ sang radian
        private double ToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
        public async Task<string> Booking(BookingRequest bookingRequest, long? tripId, float? amount, string? method, string? status)
        {
            var checkTrip = await tripRepository.GetTripByCustomerId(bookingRequest.CustomerID);
            if (checkTrip != null) 
            {
                if(checkTrip.Status == "A" || checkTrip.Status == "H" || checkTrip.Status == "T")
                {
                    return "You have made another booking";
                }
            }
            if (string.IsNullOrEmpty(method) && string.IsNullOrEmpty(amount.ToString()) && string.IsNullOrEmpty(tripId.ToString()))
            {
                double longitudePStart = 0, latitudePStart = 0, longitudeEnd = 0, latitudeEnd = 0;
                var gsp = await gSPRepository.GetGSP(bookingRequest.Source, bookingRequest.Destination);
                if (gsp == null)
                {
                    throw new Exception("GSP is not exist!!!");
                }
                else
                {
                    string[] partsPStart = gsp.PStart.Trim('(', ')').Split(',');
                    longitudePStart = double.Parse(partsPStart[0].Trim());
                    latitudePStart = double.Parse(partsPStart[1].Trim());

                    string[] partsPEnd = gsp.PEnd.Trim('(', ')').Split(',');
                    longitudeEnd = double.Parse(partsPEnd[0].Trim());
                    latitudeEnd = double.Parse(partsPEnd[1].Trim());
                }
                var tripList = await tripRepository.GetAll();
                long tripID = tripList.Any() ? tripList.Count() + 1 : 1;
                var trip = new Trip
                {
                    Id = tripID,
                    CreateAt = BitConverter.GetBytes(DateTime.Now.ToBinary()),
                    CustomerId = bookingRequest.CustomerID,
                    SourceLatitude = latitudePStart,
                    SourceLongitude = longitudePStart,
                    DestinationLatitude = latitudeEnd,
                    DestinationLongitude = longitudeEnd,
                };
                await tripRepository.Add(trip);

                double distance = CalculateDistance(latitudePStart, longitudePStart, latitudeEnd, longitudeEnd);
                double Amount = 10000 + (5000 * distance) + (500 * distance / 50) + 20000;
                return "Trip id " + trip.Id + " The amount you have to pay for the trip is " + Math.Ceiling(Amount) + " VND. Please select your payment method.";
            }
            else if (string.IsNullOrEmpty(amount.ToString()) && !string.IsNullOrEmpty(tripId.ToString()) && string.IsNullOrEmpty(method) && !string.IsNullOrEmpty(status))
            {
                Trip trip = await tripRepository.GetTripById((long)tripId);
                if(trip.Status == "A")
                {
                    return "You cannot cancel because the driver's booking has been accepted!!!";
                }
                else if(trip.Status == "P")
                {
                    return "You cannot cancel because the driver has arrived!!!";
                }
                else if (trip.Status == "T")
                {
                    return "You cannot cancel because ongoing!!!";
                }
                else if (trip.Status == "F")
                {
                    return "You cannot cancel because booking is finished!!!";
                }
                else if (trip.Status == "R")
                {
                    return "You cannot cancel because you are rated!!!";
                }

                trip.Status = "C";
                await tripRepository.Update(trip);
                return "Booking has been cancelled!!!";
            }
            else if(!string.IsNullOrEmpty(amount.ToString()) && !string.IsNullOrEmpty(tripId.ToString()) && !string.IsNullOrEmpty(method) && string.IsNullOrEmpty(status))
            {
                Driver driver = new Driver();
                var drivers = await driverRepository.GetDrivers();
                int count = 0;
                bool check = false;
                Trip trip = await tripRepository.GetTripById((long)tripId);
                if (trip == null) 
                {
                    throw new Exception("Cannot search TripId");
                }
                foreach (var item in drivers)
                {
                    count++;
                    double km = CalculateDistance((float)trip.SourceLatitude, (float)trip.SourceLongitude, (float)item.LocationLatitude, (float)item.LocationLongitude);
                    if (km <= 2)
                    {
                        driver = item;
                        check = true;
                        break;
                    }
                    if (count == 1)
                    {
                        trip.Status = "S";
                        await tripRepository.Update(trip);
                    }
                }
                var listPayment = await paymentRepository.GetAll();
                long paymentId = listPayment.Any() ? listPayment.Count + 1 : 1;
                string m;
                if (method == "Banking" || method == "banking") 
                {
                    m = "B";
                }
                else
                {
                    m = "T";
                }
                Payment payment = new Payment
                {
                    Id = paymentId,
                    TripId = tripId,
                    Amount = amount,
                    Method = m,
                    CreateAt = BitConverter.GetBytes(DateTime.Now.ToBinary())
                };
                await paymentRepository.Add(payment);
                if (check)
                {
                    trip.DriverId = driver.Id;
                    trip.Status = "D";
                    trip.PaymentId = paymentId;
                    await tripRepository.Update(trip);
                    return "Booking is success!!! Your Driver is " +trip.Driver.Id;
                }
                else
                {
                    trip.Status = "C";
                    await tripRepository.Update(trip);
                    payment.Amount = null;
                    payment.Method = null;
                    await paymentRepository.Update(payment);
                    return "There are no drivers near you within a 2km radius!!!";
                }
            }
            return null;
        }

        public async Task<string> GetStatusTrip(long tripId, long customerId , int? rating, string? feedback)
        {
            Trip trip = await tripRepository.GetTripById(tripId);
            if (trip == null)
            {
                return "You have not booked yet";
            }
            else
            {
                if (trip.Status == "A")
                {
                    Driver driver = await driverRepository.GetDriverByDriverID((long)trip.DriverId);
                    User user = await userRepository.GetUserById((long)driver.UserId);
                    return trip.Driver.User.UserName + " " + driver.DriverRating + " " + driver.Cab.RegNo + " accepted booking.";
                }
                else if(trip.Status == "H")
                {
                    return "The driver has arrived at the pick up point";
                }
                else if (trip.Status == "T")
                {
                    return "Ongoing!!!";
                }
                else if (trip.Status == "C")
                {
                    return "Driver canceled booking!!!";
                }
                else if (trip.Status == "F")
                {
                    return "Booking is finished!!!";
                }
                else if (trip.Status == "P" && string.IsNullOrEmpty(rating.ToString()) && string.IsNullOrEmpty(feedback))
                {
                    return "Yor are payment!!!";
                }
                else if (!string.IsNullOrEmpty(rating.ToString()) && !string.IsNullOrEmpty(feedback))
                {
                    if (trip.Status == "A" || trip.Status == "H" || trip.Status == "T" || trip.Status == "F")
                    {
                        return "Can't rate because it hasn't been done yet!!!";
                    }

                    var listRating = await ratingRepository.GetAllRating();
                    long ratingId = listRating.Any() ? listRating.Count + 1 : 1;
                    var newRating = new Rating
                    {
                        Id = ratingId,
                        CustomerId = customerId,
                        DriverId = trip.DriverId,
                        Feedback = feedback[0].ToString(),
                        Rating1 = rating
                    };
                    await ratingRepository.Add(newRating);

                    trip.Status = "R";
                    await tripRepository.Update(trip);

                    var averageRating = await ratingRepository.GetAverageRating((long)trip.DriverId);
                    Driver driver = await driverRepository.GetDriverByDriverID((long)trip.DriverId);
                    driver.DriverRating = (float) averageRating;
                    await driverRepository.Update(driver);
                    return "You are rated!!!";
                }
            }
            return null;
        }
    }
}
