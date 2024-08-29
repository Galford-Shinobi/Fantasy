using Fantasy.Backend.Helpers.Interfaces;
using Fantasy.Backend.UnitsOfWork.Interfaces;
using Fantasy.Shared.Entites;
using Fantasy.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Fantasy.Backend.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IFireBaseService _fireBaseService;
        private readonly IUsersUnitOfWork _usersUnitOfWork;
        private readonly IConfiguration _configuration;

        public SeedDb(DataContext context, IFireBaseService fireBaseService, IUsersUnitOfWork usersUnitOfWork, IConfiguration configuration)
        {
            _context = context;
            _fireBaseService = fireBaseService;
            _usersUnitOfWork = usersUnitOfWork;
            _configuration = configuration;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            await CheckCountriesAsync();
            await CheckTeamsAsync();
            await CheckRolesAsync();
            await CheckUsersAsync();
        }

        private async Task CheckCountriesAsync()
        {
            if (!_context.Countries.Any())
            {
                var countriesStatesCitiesSQLScript = File.ReadAllText("Data\\CountriesStatesCities.sql");
                await _context.Database.ExecuteSqlRawAsync(countriesStatesCitiesSQLScript);
            }
        }

        private async Task CheckRolesAsync()
        {
            await _usersUnitOfWork.CheckRoleAsync(UserType.Admin.ToString());
            await _usersUnitOfWork.CheckRoleAsync(UserType.User.ToString());
        }

        private async Task CheckTeamsAsync()
        {
            if (!_context.Teams.Any())
            {
                foreach (var country in _context.Countries)
                {
                    var imagePath = string.Empty;
                    var filePath = $"{Environment.CurrentDirectory}\\Images\\Flags\\{country.Name}.png";
                    string CarpetaDestino = _configuration["Configuracion:FireBase_StorageCarpeta_Teams"]!;
                    if (File.Exists(filePath))
                    {
                        imagePath = await FireBaseServiceImage(filePath, CarpetaDestino);
                    }
                    _context.Teams.Add(new Team { Name = country.Name, Country = country!, Image = imagePath });
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckUsersAsync()
        {
            await CheckUserAsync("Draco Master", "Orochi", "draco.master.orochi@yopmail.com", "322 311 4620", "DracoMaster.png", UserType.Admin);
            await CheckUserAsync("Juan", "Zuluaga", "zulu@yopmail.com", "322 311 4620", "JuanZuluaga.jpg", UserType.Admin);
            await CheckUserAsync("Ledys", "Bedoya", "ledys@yopmail.com", "322 311 4620", "LedysBedoya.jpg", UserType.User);
            await CheckUserAsync("Brad", "Pitt", "brad@yopmail.com", "322 311 4620", "Brad.jpg", UserType.User);
            await CheckUserAsync("Angelina", "Jolie", "angelina@yopmail.com", "322 311 4620", "Angelina.jpg", UserType.User);
            await CheckUserAsync("Bob", "Marley", "bob@yopmail.com", "322 311 4620", "bob.jpg", UserType.User);
            await CheckUserAsync("Celia", "Cruz", "celia@yopmail.com", "322 311 4620", "celia.jpg", UserType.Admin);
            await CheckUserAsync("Fredy", "Mercury", "fredy@yopmail.com", "322 311 4620", "fredy.jpg", UserType.User);
            await CheckUserAsync("Hector", "Lavoe", "hector@yopmail.com", "322 311 4620", "hector.jpg", UserType.User);
            await CheckUserAsync("Liv", "Taylor", "liv@yopmail.com", "322 311 4620", "liv.jpg", UserType.User);
            await CheckUserAsync("Otep", "Shamaya", "otep@yopmail.com", "322 311 4620", "otep.jpg", UserType.User);
            await CheckUserAsync("Ozzy", "Osbourne", "ozzy@yopmail.com", "322 311 4620", "ozzy.jpg", UserType.User);
            await CheckUserAsync("Selena", "Quintanilla", "selena@yopmail.com", "322 311 4620", "selena.jpg", UserType.User);
        }

        private async Task<User> CheckUserAsync(string firstName, string lastName, string email, string phone, string image, UserType userType)
        {
            var user = await _usersUnitOfWork.GetUserAsync(email);
            if (user == null)
            {
                var imagePath = string.Empty;
                Country country = new();
                var filePath = $"{Environment.CurrentDirectory}\\Images\\users\\{image}";
                var fileBytes = File.ReadAllBytes(filePath);
                string CarpetaDestino = _configuration["Configuracion:FireBase_StorageCarpeta_Usuario"]!;
                if (File.Exists(filePath))
                {
                    imagePath = await FireBaseServiceImage(filePath, CarpetaDestino);
                }
                if (email == "draco.master.orochi@yopmail.com")
                {
                    country = await _context.Countries.FirstOrDefaultAsync(x => x.Name == "Japan");
                }
                else
                {
                    country = await _context.Countries.FirstOrDefaultAsync(x => x.Name == "Colombia");
                }

                user = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    UserName = email,
                    PhoneNumber = phone,
                    Country = country!,
                    UserType = userType,
                    Photo = imagePath
                };

                await _usersUnitOfWork.AddUserAsync(user, "123456");
                await _usersUnitOfWork.AddUserToRoleAsync(user, userType.ToString());

                var token = await _usersUnitOfWork.GenerateEmailConfirmationTokenAsync(user);
                await _usersUnitOfWork.ConfirmEmailAsync(user, token);
            }

            return user;
        }

        private async Task<string> FireBaseServiceImage(string Image, string StorageCarpeta)
        {
            var imagePath = string.Empty;
            if (!string.IsNullOrEmpty(Image))
            {
                //var imageBase64 = Convert.FromBase64String(Image!);

                var filePath = Image;
                Stream StreamArchivo = File.OpenRead(filePath);
                var fileNameWithExtension = Path.GetFileName(filePath);
                imagePath = await _fireBaseService.SubirStorageAsync(StreamArchivo, StorageCarpeta, fileNameWithExtension);
            }
            return imagePath;
        }
    }
}