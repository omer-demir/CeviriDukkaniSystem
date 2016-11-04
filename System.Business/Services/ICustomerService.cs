using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangent.CeviriDukkani.Domain.Common;
using Tangent.CeviriDukkani.Domain.Dto.Sale;

namespace System.Business.Services
{
    public interface ICustomerService
    {
        ServiceResult<CustomerDto> AddCustomer(CustomerDto customerDto, int createdBy);
        ServiceResult<CustomerDto> GetCustomer(int customerId);
        ServiceResult<CustomerDto> EditCustomer(CustomerDto customerDto, int createdBy);
        ServiceResult<List<CustomerDto>> GetCustomers();
        ServiceResult<List<CustomerDto>> GetCustomersByCompanyId(int companyId);
        ServiceResult<CustomerDto> SetActive(CustomerDto customerDto);
    }
}
