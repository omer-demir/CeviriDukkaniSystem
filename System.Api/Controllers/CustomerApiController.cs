using System;
using System.Business.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Tangent.CeviriDukkani.Domain.Common;
using Tangent.CeviriDukkani.Domain.Dto.Request;
using Tangent.CeviriDukkani.Domain.Dto.Sale;
using Tangent.CeviriDukkani.WebCore.BaseControllers;

namespace System.Api.Controllers
{
    [RoutePrefix("api/customerapi")]
    public class CustomerApiController : BaseApiController
    {
        private readonly ICustomerService _customerService;

        public CustomerApiController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost, Route("addCustomer")]
        public HttpResponseMessage AddCustomer(CustomerDto customerDto)
        {
            var serviceResult = _customerService.AddCustomer(customerDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success)
            {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpPost, Route("editCustomer")]
        public HttpResponseMessage EditCustomer(CustomerDto customerDto)
        {
            var serviceResult = _customerService.EditCustomer(customerDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success)
            {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpGet, Route("getCustomers")]
        public HttpResponseMessage GetCustomers()
        {
            var serviceResult = _customerService.GetCustomers();
            if (serviceResult.ServiceResultType != ServiceResultType.Success)
            {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpGet, Route("getCustomer")]
        public HttpResponseMessage GetCustomer([FromUri]int customerId)
        {
            var serviceResult = _customerService.GetCustomer(customerId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success)
            {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpGet, Route("getCustomersByCompanyId")]
        public HttpResponseMessage GetCustomersByCompanyId([FromUri]int companyId)
        {
            var serviceResult = _customerService.GetCustomersByCompanyId(companyId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success)
            {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpPost, Route("setActive")]
        public HttpResponseMessage SetActive(CustomerDto customerDto)
        {
            var serviceResult = _customerService.SetActive(customerDto);
            if (serviceResult.ServiceResultType != ServiceResultType.Success)
            {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
    }
}
