using Fantasy.Backend.Data;
using Fantasy.Backend.Helpers.Interfaces;
using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Fantasy.Backend.Repositories.Implementations
{
    public class UsersRepository : IUsersRepository
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IFireBaseService _fireBaseService;
        private readonly IConfiguration _configuration;

        public UsersRepository(DataContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager, IFireBaseService fireBaseService, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _fireBaseService = fireBaseService;
            _configuration = configuration;
        }

        public async Task<User> GetUserAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Country)
                .FirstOrDefaultAsync(x => x.Id == userId.ToString());
            return user!;
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<SignInResult> LoginAsync(LoginDTO model)
        {
            return await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, true);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            if (!string.IsNullOrEmpty(user.Photo))
            {
                var imageBase64 = Convert.FromBase64String(user.Photo!);
                string contenido = user.Photo;
                byte[] byteArray = Encoding.UTF8.GetBytes(contenido);
                Stream StreamArchivo = new MemoryStream(byteArray);
                string CarpetaDestino = _configuration["Configuracion:IMAGENES_USUARIO"]!;
                string NombreArchivo = user.Photo;
                user.Photo = await _fireBaseService.SubirStorageAsync(StreamArchivo, CarpetaDestino, NombreArchivo);
            }

            var result = await _userManager.CreateAsync(user, password);
            return result;
        }

        public async Task AddUserToRoleAsync(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task CheckRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName
                });
            }
        }

        public async Task<User> GetUserAsync(string email)
        {
            var user = await _context.Users
                .Include(u => u.Country)
                .FirstOrDefaultAsync(x => x.Email == email);
            return user!;
        }

        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }
    }
}