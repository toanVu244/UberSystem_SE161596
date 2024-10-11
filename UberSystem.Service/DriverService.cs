using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Interfaces;
using UberSystem.Domain.Interfaces.Services;
using UberSystem.Domain.Repository;
using UberSystem.Infrastructure;
using UberSystem.Infrastructure.Repository;

namespace UberSystem.Service
{
    public interface IDriverService
    {
        Task<string> NortificationBookingforDriver(long driverID, long? tripId , string? response);
        Task<string> UpdateLocation(long driverID);
        Task GetDriverRequestAsync(long driverId);
    }
    public class DriverService : IDriverService
    {
        private readonly IDriverRepository driverRepository;
        private readonly ITripRepository tripRepository;
        private readonly IUserRepository userRepository;
        private readonly IGSPRepository gSPRepository;
        private readonly IPaymentRepository paymentRepository;


        public DriverService(IDriverRepository driverRepository, ITripRepository tripRepository, IUserRepository userRepository, IGSPRepository gSPRepository, IPaymentRepository paymentRepository)
        {
            this.driverRepository = driverRepository;   
            this.tripRepository = tripRepository;
            this.userRepository = userRepository;
            this.gSPRepository = gSPRepository;
            this.paymentRepository = paymentRepository;
        }
        public async Task GetDriverRequestAsync(long driverId)
        {
            Console.WriteLine($"Requesting driver data for driver ID: {driverId}");

            var loca = await gSPRepository.GetlocaRan();

            loca = loca.Trim('(', ')');
            string[] parts = loca.Split(',');

            double latitude = double.Parse(parts[0].Trim());
            double longitude = double.Parse(parts[1].Trim());
            await driverRepository.UpdateLocationAsync(driverId, latitude, longitude);
        }

        public async Task<string> NortificationBookingforDriver(long driverID, long? tripId , string? response)
        {
            if (string.IsNullOrWhiteSpace(response) && string.IsNullOrEmpty(tripId.ToString()))
            {
                Trip trip = await tripRepository.GetTripByDriverId(driverID);
                if (trip == null)
                {
                    return "You don't have a booking yet";
                }
                else
                {
                    User user = await userRepository.GetUserFromCustomerId((long)trip.CustomerId);
                    string location = "" + trip.SourceLongitude + "," + trip.SourceLatitude + "";
                    GSP gsp = await gSPRepository.GetGSPByPStart(location); 
                    return "Trip id" + trip.Id + "You have a booking from " + user.UserName + " are staying " + gsp.Regions.Split(',')[0].Trim();
                }
            }
            else if (!string.IsNullOrWhiteSpace(response) && !string.IsNullOrEmpty(tripId.ToString()))
            {
                Trip trip = await tripRepository.GetTripById((long)tripId);
                Driver driver = await driverRepository.GetDriverByDriverID(driverID);
                if(response == "Accept" || response == "accept")
                {
                    trip.DriverId = driverID;
                    trip.Status = "A";
                    await tripRepository.Update(trip);

                    return "You have accepted the booking!!!";
                }
                if (response == "Arrived" || response == "arrived")
                {
                    trip.DriverId = driverID;
                    trip.Status = "H";
                    await tripRepository.Update(trip);

                    driver.LocationLatitude = trip.SourceLatitude;
                    driver.LocationLongitude = trip.SourceLongitude;

                    await driverRepository.Update(driver);

                    return "The driver has arrived at the set point!!!";
                }
                if (response == "Carried" || response == "carried")
                {
                    trip.DriverId = driverID;
                    trip.Status = "T";
                    await tripRepository.Update(trip);

                    string pStart = $"({trip.SourceLongitude}, {trip.SourceLatitude})";
                    string pEnd = $"({trip.DestinationLongitude}, {trip.DestinationLatitude})";
                    GSP gSP = await gSPRepository.GetGSPByPStartPEnd(pStart, pEnd);

                    string coordinates = gSP.PTerm.Trim('(', ')');
                    string[] parts = coordinates.Split(',');

                    double longitude = double.Parse(parts[0].Trim());
                    double latitude = double.Parse(parts[1].Trim());

                    driver.LocationLatitude = latitude;
                    driver.LocationLongitude = longitude;

                    await driverRepository.Update(driver);

                    return "Transported passengers!!!";
                }
                else if(response == "Finished" || response == "finished")
                {
                    trip.Status = "F";
                    await tripRepository.Update(trip);

                    driver.LocationLatitude = trip.DestinationLatitude;
                    driver.LocationLongitude = trip.DestinationLongitude;

                    await driverRepository.Update(driver);

                    return "Booking is finished!!!";
                }
                else if (response == "Payment" || response == "payment")
                {
                    trip.Status = "P";
                    await tripRepository.Update(trip);
                    return "Booking is payment!!!";
                }
                else if(response == "Cancel" || response == "cancel")
                {
                    trip.Status = "C";
                    await tripRepository.Update(trip);
                    Payment resetPayment = await paymentRepository.GetByTripId(trip.Id);
                    if (resetPayment != null) 
                    { 
                        resetPayment.Method = null;
                        resetPayment.Amount = null;
                        await paymentRepository.Update(resetPayment);
                    }
                    return "You have cancel the booking!!!";
                }
            }

            return null;
        }

        private async Task<User> GetUser(long driverId)
        {
            Driver driver = await driverRepository.GetDriverByDriverID(driverId);
            return await userRepository.GetUserById((long)driver.UserId);
        }

        public async Task<string> UpdateLocation(long driverID)
        {
            Driver driver = await driverRepository.GetDriverByDriverID((long)driverID);
            return driver.LocationLatitude + " " + driver.LocationLongitude;
        }
    }
}
