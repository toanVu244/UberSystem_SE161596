using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Interfaces;
using UberSystem.Domain.Interfaces.Services;
using UberSystem.Domain.Models;
using UberSystem.Domain.Repository;
using UberSystem.Domain.Request;
using UberSystem.Infrastructure.Model;
using UberSystem.Infrastructure.Repository;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;
namespace UberSystem.Service
{
	public class UserService : IUserService
	{
        private readonly IUserRepository userRepository;
        private readonly IDriverRepository driverRepository;
        private readonly ICabRepsitory cabRepsitory;
        private readonly ICustomerRepository customerRepository;
        private readonly IRefreshTokenRepository refreshRepository;
        private readonly IGSPRepository gspRepository;
        private readonly IConfiguration configuration;
        private readonly IEmailService emailService;

        public UserService(IUserRepository userRepository, IDriverRepository driverRepository, ICabRepsitory cabRepsitory, ICustomerRepository customerRepository, IConfiguration configuration, IRefreshTokenRepository refreshRepository, IEmailService emailService, IGSPRepository gSPRepository)
        {
            this.userRepository = userRepository;
            this.driverRepository = driverRepository;
            this.cabRepsitory = cabRepsitory;
            this.customerRepository = customerRepository;
            this.configuration = configuration;
            this.refreshRepository = refreshRepository;
            this.emailService = emailService;
            this.gspRepository = gSPRepository;
        }

        private string GenerateCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();

            string code = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return code;
        }


        public async Task<String> Add(AddUserRequest userRequest, string? code)
        {
            var checkEmail = await userRepository.GetUserByEmail(userRequest.Email);
            if (checkEmail != null && checkEmail.Status)
            {
                return ("Email is existed!!!");
            }
            if (string.IsNullOrEmpty(code))
            {
                var listUser = await userRepository.GetAllUser();
                int countUser = listUser.Any() ? listUser.Count + 1 : 1;
                string randomCode = GenerateCode();
                User user = new User
                {
                    Id = countUser,
                    UserName = userRequest.UserName,
                    Email = userRequest.Email,
                    Password = userRequest.Password,
                    Role = userRequest.Role,
                    Status = false,
                    Code = randomCode   
                };
                await userRepository.AddUser(user);
                await emailService.SendEmailAsync(userRequest.Email, "Confirm your account", $"Here is your code: {randomCode}. Please enter this code to authenticate your account.");
                return "The system has sent the code via email. Please enter the code!!!";
            }
            else
            {
                var checkCode = await userRepository.GetUserByCode(code);
                if (checkCode == null)
                {
                    return "Code is wrong";
                }
                else
                {
                    checkCode.Status = true;
                    await userRepository.UpdateUser(checkCode);
                    if (userRequest.Role == "Driver")
                    {
                        if (string.IsNullOrEmpty(userRequest.DriverRequest.CabRequest.RegNo) || string.IsNullOrEmpty(userRequest.DriverRequest.CabRequest.Type))
                        {
                            throw new Exception("RegNo or Type can not null!!!");
                        }
                        
                        string[] dateFormats = { "dd/MM/yyyy", "d/MM/yyyy", "dd/M/yyyy", "d/M/yyyy" };
                        DateTime Dob;
                        bool isValidDate = DateTime.TryParseExact(userRequest.DriverRequest.Dob, dateFormats,
                                                                  System.Globalization.CultureInfo.InvariantCulture,
                                                                  System.Globalization.DateTimeStyles.None,
                                                                  out Dob);

                        if (!isValidDate)
                        {
                            throw new FormatException("Date format is incorrect. Please enter the date in the format dd/MM/yyyy, d/MM/yyyy, dd/M/yyyy, d/M/yyyy.");
                        }



                        var listDriver = await driverRepository.GetDrivers();
                        int countDriver = listDriver.Any() ? listDriver.Count + 1 : 1;

                        GSP currenLocation = await gspRepository.GetGSPByLocation(userRequest.DriverRequest.CurrentLocation);

                        string coordinates = currenLocation.PStart.Trim('(', ')');
                        string[] parts = coordinates.Split(',');

                        double longitude = double.Parse(parts[0].Trim());
                        double latitude = double.Parse(parts[1].Trim());
                        var driver = new Driver
                        {
                            Id = countDriver,
                            CreateAt = BitConverter.GetBytes(DateTime.Now.ToBinary()),
                            Dob = Dob,
                            DriverRating = 0,
                            UserId = checkCode.Id,
                            LocationLatitude = latitude,
                            LocationLongitude = longitude,  
                        };
                        await driverRepository.Add(driver);

                        var listCab = await cabRepsitory.GetAll();
                        int countCab = listCab.Any() ? listCab.Count + 1 : 1;

                        var cab = new Cab
                        {
                            Id = countCab,
                            Type = userRequest.DriverRequest.CabRequest.Type,
                            RegNo = userRequest.DriverRequest.CabRequest.RegNo,
                        };
                        await cabRepsitory.Add(cab);

                        driver.CabId = cab.Id;
                        await driverRepository.Update(driver);

                        cab.DriverId = driver.Id;
                        await cabRepsitory.Update(cab);

                        return "Driver registered successfully";
                    }
                    if (userRequest.Role == "Customer")
                    {
                        var listCustomer = await customerRepository.GetAll();
                        int coutCustomer = listCustomer.Any() ? listCustomer.Count + 1 : 1;
                        var customer = new Customer
                        {
                            Id = coutCustomer,
                            CreateAt = BitConverter.GetBytes(DateTime.Now.ToBinary()),
                            UserId = checkCode.Id,
                        };
                        await customerRepository.Add(customer);
                        return "Customer registered successfully!!!";
                    }
                }
            }

            return null;
        }

        public Task CheckPasswordAsync(User user)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(long id)
        {
            var user = await userRepository.GetUserById(id);
            if (user == null)
            {
                throw new Exception("User is not exist!!!");
            }
            user.Status = false;

            await userRepository.UpdateUser(user);
        }

        public async Task<User> FindByEmail(string email)
        {
            User user = await userRepository.GetUserByEmail(email);
            if(user != null)
            {
                if (user.Status)
                {
                    return user;
                }
            }
            return null;
        }

        public async Task Update(UserRequest userRequest)
        {
            User checkUser = await userRepository.GetUserById(userRequest.Id);
            if (checkUser == null)
            {
                throw new Exception("User is not exist!!!");
            }
            checkUser.Status = userRequest.Staus;
            checkUser.Email = userRequest.Email;
            checkUser.Password = userRequest.Password; 
            checkUser.UserName = userRequest.UserName;
            await userRepository.UpdateUser(checkUser);
        }

        public async Task<TokenModel> Login(LoginRequest loginRequest)
        {
            //var user = await userRepository.Login(loginRequest.UserName, loginRequest.Password);
            //if (user == null)
            //{
            //    throw new Exception("Email or password is wrong!!!");
            //}
            //else if (!user.Status)
            //{
            //    throw new Exception("Unverified account!!!");
            //}
            //var token = await GenerateToken(user);
            //return token;
            var user = await userRepository.Login(loginRequest.UserName, loginRequest.Password);
            if (user == null)
            {
                throw new Exception("Email or password is wrong!!!");
            }
            else if (!user.Status)
            {
                throw new Exception("Unverified account!!!");
            }

            // Tạo token và thêm userId vào TokenModel
            var token = await GenerateToken(user);
            token.UserId = user.Id; 
            token.Role = user.Role; 
            return token;
        }

        private async Task<TokenModel> GenerateToken(User user)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("SecretKey is not configured in JwtSettings.");
            }
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescripsion = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserName", user.UserName),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("Id", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            var token = jwtHandler.CreateToken(tokenDescripsion);
            var accessToken = jwtHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            RefreshToken checkRefreshToken = await refreshRepository.GetRefreshTokenByUserID(user.Id);
            if (checkRefreshToken == null)
            {

                var refreshTokenEntity = new RefreshToken
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    JwtId = token.Id,
                    Token = refreshToken,
                    IsUsed = false,
                    IsRevoked = false,
                    IssuedAt = DateTime.UtcNow,
                    ExpireAt = DateTime.UtcNow.AddHours(1)
                };

                await refreshRepository.Add(refreshTokenEntity);
            }
            else
            {
                checkRefreshToken.Token = refreshToken;

                await refreshRepository.Update(checkRefreshToken);
            }

            return new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }

        private string GenerateRefreshToken()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
                return Convert.ToBase64String(random);
            }
        }

        public async Task<TokenModel> RenewToken(TokenModel tokenModel)
        {
            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtSettings = configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"];
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                var tokenParameter = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    RoleClaimType = "Role"
                };

                if (string.IsNullOrEmpty(secretKey))
                {
                    throw new InvalidOperationException("SecretKey is not configured in JwtSettings.");
                }

                var tokenVerification = jwtHandler.ValidateToken(tokenModel.AccessToken, tokenParameter, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);
                    if (!result)
                    {
                        throw new Exception("Invalid token !!!");
                    }
                }

                var utcExpireDate = long.Parse(tokenVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expireDate = ConverUnixTimeToDateTime(utcExpireDate);
                if (expireDate > DateTime.UtcNow)
                {
                    throw new Exception("Access token has not yet expired!!!");
                }

                RefreshToken storedToken = await refreshRepository.GetRefreshTokenByToken(tokenModel.RefreshToken);
                if (storedToken == null)
                {
                    throw new Exception("Refresh token does not exist!!!");
                }

                if (storedToken.IsUsed)
                {
                    throw new Exception("Refresh token has been used!!!");
                }

                if (storedToken.IsRevoked)
                {
                    throw new Exception("Refresh token has been revoked!!!");
                }

                var jti = tokenVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti)
                {
                    throw new Exception("Token does not match!!!");
                }

                storedToken.IsUsed = true;
                storedToken.IsRevoked = true;

                await refreshRepository.Update(storedToken);

                var user = await userRepository.GetUserById(storedToken.UserId);
                var token = await GenerateToken(user);
                return token;

            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        private DateTime ConverUnixTimeToDateTime(long utcExpireDate)
        {
            var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(utcExpireDate).ToUniversalTime();
            return dateTimeInterval;
        }
    }
}

