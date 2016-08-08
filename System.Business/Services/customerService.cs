using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Tangent.CeviriDukkani.Data.Model;
using Tangent.CeviriDukkani.Domain.Common;
using Tangent.CeviriDukkani.Domain.Dto.Enums;
using Tangent.CeviriDukkani.Domain.Dto.Sale;
using Tangent.CeviriDukkani.Domain.Entities.Sale;
using Tangent.CeviriDukkani.Domain.Exceptions;
using Tangent.CeviriDukkani.Domain.Exceptions.ExceptionCodes;
using Tangent.CeviriDukkani.Domain.Mappers;


namespace System.Business.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly CeviriDukkaniModel _model;
        private readonly CustomMapperConfiguration _customMapperConfiguration;
        private readonly ILog _logger;
        //private readonly IMailService _mailService;

        public CustomerService(CeviriDukkaniModel model, CustomMapperConfiguration customMapperConfiguration, ILog logger)
        {
            _model = model;
            _customMapperConfiguration = customMapperConfiguration;
            _logger = logger;
            //_mailService = new YandexMailService();
        }
        public ServiceResult<CustomerDto> AddCustomer(CustomerDto customerDto, int createdBy)
        {
            var serviceResult = new ServiceResult<CustomerDto>();
            try
            {
                customerDto.CreatedBy = createdBy;
                customerDto.Active = true;

                var customer = _customMapperConfiguration.GetMapEntity<Customer, CustomerDto>(customerDto);

                _model.Customers.Add(customer);
                if (_model.SaveChanges() <= 0)
                {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<CustomerDto, Customer>(customer);
            }
            catch (Exception exc)
            {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult<CustomerDto> GetCustomer(int customerId)
        {
            var serviceResult = new ServiceResult<CustomerDto>();
            try
            {
                var customer = _model.Customers.Find(customerId);
                if (customer == null)
                {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<CustomerDto, Customer>(customer);
            }
            catch (Exception exc)
            {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult<CustomerDto> EditCustomer(CustomerDto customerDto, int createdBy)
        {
            var serviceResult = new ServiceResult<CustomerDto>();
            try
            {
                var customer = _model.Customers.FirstOrDefault(f => f.Id == customerDto.Id);
                if (customer == null)
                {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }
                customer.Email = customerDto.Email;
                customer.MobilePhone = customerDto.MobilePhone;
                customer.Name = customerDto.Name;
                customer.Password = customerDto.Password;
                customer.CompanyId = customerDto.MembershipTypeId == (int)MembershipTypeEnum.Personal ? null : customerDto.CompanyId;
                customer.InstitutionCode = customerDto.InstitutionCode;
                customer.MembershipTypeId = customerDto.MembershipTypeId;
                customer.Surname = customerDto.Surname;

                customerDto.UpdatedBy = createdBy;
                customerDto.UpdatedAt = DateTime.Now;

                _model.SaveChanges();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<CustomerDto, Customer>(customer);
            }
            catch (Exception exc)
            {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult<List<CustomerDto>>  GetCustomers()
        {
            var serviceResult = new ServiceResult<List<CustomerDto>>();
            try
            {
                var customers = _model.Customers.ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = customers.Select(s => _customMapperConfiguration.GetMapDto<CustomerDto, Customer>(s)).ToList();
            }
            catch (Exception exc)
            {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult<List<CustomerDto>> GetCustomersByCompanyId(int companyId)
        {
            var serviceResult = new ServiceResult<List<CustomerDto>>();
            try
            {
                var customers = _model.Customers.Where(w => w.CompanyId == companyId).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = customers.Select(s => _customMapperConfiguration.GetMapDto<CustomerDto, Customer>(s)).ToList();
            }
            catch (Exception exc)
            {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
    }
}
