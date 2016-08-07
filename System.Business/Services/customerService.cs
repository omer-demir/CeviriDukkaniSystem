using System;
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
        internal ILog Log { get; } = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly CeviriDukkaniModel _ceviriDukkaniModel;
        private readonly ICustomMapperConfiguration _customMapperConfiguration;

        public CustomerService(CeviriDukkaniModel ceviriDukkaniModel, ICustomMapperConfiguration customMapperConfiguration)
        {
            _ceviriDukkaniModel = ceviriDukkaniModel;
            _customMapperConfiguration = customMapperConfiguration;
        }

        public ServiceResult<CustomerDto> AddCustomer(CustomerDto customerDto, int createdBy)
        {
            var serviceResult = new ServiceResult<CustomerDto>();
            try
            {
                customerDto.CreatedBy = createdBy;
                customerDto.Active = true;

                var customer = _customMapperConfiguration.GetMapEntity<Customer, CustomerDto>(customerDto);

                _ceviriDukkaniModel.Customers.Add(customer);
                if (_ceviriDukkaniModel.SaveChanges() <= 0)
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
                Log.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult<CustomerDto> GetCustomer(int customerId)
        {
            var serviceResult = new ServiceResult<CustomerDto>();
            try
            {
                var customer = _ceviriDukkaniModel.Customers.Find(customerId);
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
                Log.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult<CustomerDto> EditCustomer(CustomerDto customerDto, int createdBy)
        {
            var serviceResult = new ServiceResult<CustomerDto>();
            try
            {
                var customer = _ceviriDukkaniModel.Customers.FirstOrDefault(f => f.Id == customerDto.Id);
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

                _ceviriDukkaniModel.SaveChanges();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<CustomerDto, Customer>(customer);
            }
            catch (Exception exc)
            {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                Log.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult<List<CustomerDto>>  GetCustomers()
        {
            var serviceResult = new ServiceResult<List<CustomerDto>>();
            try
            {
                var customers = _ceviriDukkaniModel.Customers.ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = customers.Select(s => _customMapperConfiguration.GetMapDto<CustomerDto, Customer>(s)).ToList();
            }
            catch (Exception exc)
            {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                Log.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult<List<CustomerDto>> GetCustomersByCompanyId(int companyId)
        {
            var serviceResult = new ServiceResult<List<CustomerDto>>();
            try
            {
                var customers = _ceviriDukkaniModel.Customers.Where(w => w.CompanyId == companyId).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = customers.Select(s => _customMapperConfiguration.GetMapDto<CustomerDto, Customer>(s)).ToList();
            }
            catch (Exception exc)
            {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                Log.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
    }
}
