using System;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Models;
using UberSystem.Domain.Request;

namespace UberSystem.Domain.Interfaces.Services
{
	public interface IUserService
	{
        Task<User> FindByEmail(string  email);
        Task Update(UserRequest userRequest);
        Task<string> Add(AddUserRequest userRequest, string? code);
        //Task<bool> Login(User user);
        Task CheckPasswordAsync(User user);
        Task Delete(long id);
        Task<TokenModel> Login(LoginRequest loginRequest);
        Task<TokenModel> RenewToken(TokenModel tokenModel);
    }
}

