using Otus.Teaching.Pcf.Administration.Core.Abstractions.Repositories;
using Otus.Teaching.Pcf.Administration.Core.Abstractions.Services;
using Otus.Teaching.Pcf.Administration.Core.Domain.Administration;
using System;
using System.Threading.Tasks;

namespace Otus.Teaching.Pcf.Administration.WebHost.Services
{
    public class EmployeeService(IRepository<Employee> employeeRepository) : IEmployeeService
    {
        public async Task UpdateAppliedPromocodes(Guid id)
        {
            var employee = await employeeRepository.GetByIdAsync(id) ?? throw new Exception("Employee doesn't exists");
            employee.AppliedPromocodesCount++;
            await employeeRepository.UpdateAsync(employee);
        }
    }
}
