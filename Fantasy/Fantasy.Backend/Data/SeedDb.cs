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
            //await CheckUserAsync("Juan", "Zuluaga", "zulu@yopmail.com", "322 311 4620", UserType.Admin);
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
                        var fileBytes = File.ReadAllBytes(filePath);
                        Stream StreamArchivo = File.OpenRead(filePath);
                        var fileNameWithExtension = Path.GetFileName(filePath);
                        imagePath = await _fireBaseService.SubirStorageAsync(StreamArchivo, CarpetaDestino, fileNameWithExtension);
                    }
                    _context.Teams.Add(new Team { Name = country.Name, Country = country!, Image = imagePath });
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}