using Application.DTOs.RefreshToken;
using Application.DTOs.User;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Utilities;
using System.Security.Claims;

namespace Application.Services
{
    public class UserService
    {
        public IUserRepository _userRepository;
        public IMapper _mapper;
        private readonly IImageStorageService _imageStorage;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly TokenService _tokenService;

        public UserService(IUserRepository userRepository, IMapper mapper,
            IImageStorageService imageStorageService, 
            IRefreshTokenRepository refreshTokenRepository,TokenService tokenService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _imageStorage = imageStorageService;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
        }

        public async Task CreateUserAsync(UserCreateDTO userCreateDTO, Stream imageStream)
        {
            // Map UserCreateDTO to User entity
            var imageUrl = await _imageStorage.SaveImageAsync(imageStream);

            var user = _mapper.Map<User>(userCreateDTO);
            user.PhotoUrl = imageUrl;
            await _userRepository.AddAsync(user);
        }

        public async Task<UserReadDTO?> GetByEmailAndPasswordAsync(string email, string password)
        {
            User? user = await _userRepository.GetByEmailAndPasswordAsync(email, password);
            if (user == null) return null;
            UserReadDTO userDTO = _mapper.Map<UserReadDTO>(user);

            //Add the Access Token
            string token = _tokenService.GenerateAccessToken(user);
            userDTO.Token = token;

            RefreshToken? activeRefreshToken = await _refreshTokenRepository
                .GetActiveRefreshToken(user.Id);
            if (activeRefreshToken == null)
            {
                RefreshToken newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);
                userDTO.RefreshToken = newRefreshToken.Token;
                userDTO.RefreshTokenExpiration = newRefreshToken.ExpiresOn;
                await _refreshTokenRepository.AddAsync(newRefreshToken);
            }
            else
            {
                userDTO.RefreshToken = activeRefreshToken.Token;
                userDTO.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
            }

            return userDTO;
        }

        public async Task<UserReadDTO?> RefreshTokens(string refreshToken, int userId)
        {

            RefreshToken? currentRefreshToken = await _refreshTokenRepository.GetRefreshTokenByTokenAndUserId(userId,refreshToken); 
            if(currentRefreshToken == null || !currentRefreshToken.IsActive)
            {
                throw new BadRequestException("Invalid refresh token");
            }
            User user = currentRefreshToken.User;
            UserReadDTO userDTO = _mapper.Map<UserReadDTO>(user);

            // Generate new access token
            userDTO.Token = _tokenService.GenerateAccessToken(user);

            //Revoke the old refresh token
            currentRefreshToken.RevokedOn = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(currentRefreshToken);

            // Generate new refresh token
            RefreshToken newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);
            userDTO.RefreshToken = newRefreshToken.Token;
            userDTO.RefreshTokenExpiration = newRefreshToken.ExpiresOn;
            await _refreshTokenRepository.AddAsync(newRefreshToken);
            return userDTO;
        }

        public async Task<UserReadDTO?> RefreshTokens(string refreshToken)
        {

            RefreshToken? currentRefreshToken = await _refreshTokenRepository.GetRefreshTokenByToken(refreshToken);
            if (currentRefreshToken == null || !currentRefreshToken.IsActive)
            {
                throw new BadRequestException("Invalid refresh token");
            }
            User user = currentRefreshToken.User;
            UserReadDTO userDTO = _mapper.Map<UserReadDTO>(user);

            // Generate new access token
            userDTO.Token = _tokenService.GenerateAccessToken(user);

            //Revoke the old refresh token
            currentRefreshToken.RevokedOn = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(currentRefreshToken);

            // Generate new refresh token
            RefreshToken newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);
            userDTO.RefreshToken = newRefreshToken.Token;
            userDTO.RefreshTokenExpiration = newRefreshToken.ExpiresOn;
            await _refreshTokenRepository.AddAsync(newRefreshToken);
            return userDTO;
        }

    }
}
