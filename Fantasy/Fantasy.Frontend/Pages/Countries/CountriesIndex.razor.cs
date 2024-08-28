using CurrieTechnologies.Razor.SweetAlert2;
using Fantasy.Frontend.Repositories;
using Fantasy.Shared.Entites;
using Fantasy.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Fantasy.Frontend.Pages.Countries
{
    public partial class CountriesIndex
    {
        [Inject] private IRepository Repository { get; set; } = null!;
        [Inject] private IStringLocalizer<Literals> Localizer { get; set; } = null!;
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;

        //[Inject] private SweetAlertService SweetAlertService { get; set; } = null!;
        private List<Country>? Countries { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            var responseHppt = await Repository.GetAsync<List<Country>>("api/countries");
            if (responseHppt.Error)
            {
                var message = await responseHppt.GetErrorMessageAsync();
                //await SweetAlertService.FireAsync(Localizer["Error"], message, SweetAlertIcon.Error);
                return;
            }
            Countries = responseHppt.Response!;
        }

        private async Task DeleteAsync(Country country)
        {
            await LoadAsync();
        }
    }
}