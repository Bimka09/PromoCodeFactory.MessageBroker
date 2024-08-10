using Otus.Teaching.Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Otus.Teaching.Pcf.GivingToCustomer.Core.Abstractions.Services;
using Otus.Teaching.Pcf.GivingToCustomer.Core.Domain;
using Otus.Teaching.Pcf.GivingToCustomer.WebHost.Mappers;
using Otus.Teaching.Pcf.GivingToCustomer.WebHost.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Otus.Teaching.Pcf.GivingToCustomer.WebHost.Services
{
    public class PromocodeService(IRepository<PromoCode> promoCodesRepository, IRepository<Preference> preferencesRepository, IRepository<Customer> customersRepository) : IPromocodeService<GivePromoCodeRequest>
    {
        private readonly IRepository<PromoCode> _promoCodesRepository = promoCodesRepository;
        private readonly IRepository<Preference> _preferencesRepository = preferencesRepository;
        private readonly IRepository<Customer> _customersRepository = customersRepository;

        public async Task GivePromoCodesToCustomersWithPreferenceAsync(GivePromoCodeRequest request)
        {
            var preference = await _preferencesRepository.GetByIdAsync(request.PreferenceId) ?? throw new Exception("preference doesn't exists");

            var customers = await _customersRepository.GetWhere(d => d.Preferences.Any(x => x.Preference.Id == preference.Id));
            var promoCode = PromoCodeMapper.MapFromModel(request, preference, customers);
            await _promoCodesRepository.AddAsync(promoCode);
        }
    }
}
