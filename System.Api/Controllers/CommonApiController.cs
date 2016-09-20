using System.Business.Services;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Tangent.CeviriDukkani.Domain.Common;
using Tangent.CeviriDukkani.Domain.Dto.Common;
using Tangent.CeviriDukkani.Domain.Dto.Request;
using Tangent.CeviriDukkani.Domain.Dto.Sale;
using Tangent.CeviriDukkani.Domain.Dto.System;
using Tangent.CeviriDukkani.Domain.Dto.Translation;
using Tangent.CeviriDukkani.Domain.Entities.Common;
using Tangent.CeviriDukkani.WebCore.BaseControllers;

namespace System.Api.Controllers {
    [RoutePrefix("api/commonapi")]
    public class CommonApiController : BaseApiController {
        private readonly ICommonService _commonService;

        public CommonApiController(ICommonService commonService) {
            _commonService = commonService;
        }

        [Route("login"), HttpPost]
        public HttpResponseMessage Login(LoginRequestDto loginRequest) {
            
            var response = new HttpResponseMessage();
            ServiceResult<UserDto> serviceResult = _commonService.Login(loginRequest.Email, loginRequest.Password);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }
            return OK(serviceResult);
        }
        [Route("changePassword"), HttpPost]
        public HttpResponseMessage ChangePassword([FromBody]ChangePasswordRequestDto changePasswordRequest) {
            ServiceResult<UserDto> serviceResult = _commonService.ChangePassword(changePasswordRequest.Email, changePasswordRequest.OldPassword, changePasswordRequest.NewPassword);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpPost, Route("addMessage")]
        public HttpResponseMessage AddMessage(MessageRequestDto messageDto) {
            var serviceResult = _commonService.AddMessage(messageDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getIncomingMessagesByUser")]
        public HttpResponseMessage GetIncomingMessagesByUser([FromUri]int userId) {
            var serviceResult = _commonService.GetIncomingMessagesByUser(userId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getSentMessagesByUser")]
        public HttpResponseMessage GetSentMessagesByUser([FromUri]int userId) {
            var serviceResult = _commonService.GetSentMessagesByUser(userId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getMessage")]
        public HttpResponseMessage GetMessage([FromUri]int messageId) {
            var serviceResult = _commonService.GetMessage(messageId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpPost, Route("getMessageByQuery")]
        public HttpResponseMessage GetMessageByQuery(Expression<Func<Message, bool>> expression)
        {
            var serviceResult = _commonService.GetMessageByQuery(expression);
            if (serviceResult.ServiceResultType != ServiceResultType.Success)
            {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("updateMessageForReadDate")]
        public HttpResponseMessage UpdateMessageForReadDate([FromUri]int messageId) {
            var serviceResult = _commonService.UpdateMessageForReadDate(messageId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("deleteSentMessage")]
        public HttpResponseMessage DeleteSentMessage([FromUri]int messageId) {
            var serviceResult = _commonService.DeleteSentMessage(messageId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("deleteIncomingMessage")]
        public HttpResponseMessage DeleteIncomingMessage([FromUri]int messageId) {
            var serviceResult = _commonService.DeleteIncomingMessage(messageId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getCompanies")]
        public HttpResponseMessage GetCompanies() {
            var serviceResult = _commonService.GetCompanies();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpPost, Route("addCompany")]
        public HttpResponseMessage AddCompany(CompanyDto companyDto) {
            var serviceResult = _commonService.AddCompany(companyDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpPost, Route("editCompany")]
        public HttpResponseMessage EditCompany(CompanyDto companyDto) {
            var serviceResult = _commonService.UpdateCompany(companyDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getLanguages")]
        public HttpResponseMessage GetLanguages() {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.GetLanguages();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpPost, Route("addLanguage")]
        public HttpResponseMessage AddLanguage(LanguageDto languageDto) {
            var serviceResult = _commonService.AddLanguage(languageDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpPost, Route("editLanguage")]
        public HttpResponseMessage EditLanguage(LanguageDto languageDto) {
            var serviceResult = _commonService.UpdateLanguage(languageDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getTargetLanguages")]
        public HttpResponseMessage GetTargetLanguages([FromUri]int sourceLanguageId) {
            var serviceResult = _commonService.GetTargetLanguages(sourceLanguageId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpPost, Route("addSourceTargetLanguages")]
        public HttpResponseMessage AddSourceTargetLanguages(SourceTargetLanguageDto sourceTargetLanguageDto) {
            var serviceResult = _commonService.AddSourceTargetLanguages(sourceTargetLanguageDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpPost, Route("deleteSourceTargetLanguages")]
        public HttpResponseMessage DeleteSourceTargetLanguages(SourceTargetLanguageDto sourceTargetLanguageDto) {
            var serviceResult = _commonService.DeleteSourceTargetLanguages(sourceTargetLanguageDto);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getTerminologies")]
        public HttpResponseMessage GetTerminologies() {
            var serviceResult = _commonService.GetTerminologies();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpPost, Route("addTerminology")]
        public HttpResponseMessage AddTerminology(TerminologyDto terminologyDto) {
            var serviceResult = _commonService.AddTerminology(terminologyDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpPost, Route("editTerminology")]
        public HttpResponseMessage EditTerminology(TerminologyDto terminologyDto) {
            var serviceResult = _commonService.UpdateTerminology(terminologyDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getPriceLists")]
        public HttpResponseMessage GetPriceLists() {
            var serviceResult = _commonService.GetPriceLists();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpPost, Route("addPriceList")]
        public HttpResponseMessage AddPriceList(PriceListDto priceListDto) {
            var serviceResult = _commonService.AddPriceList(priceListDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpPost, Route("editPriceList")]
        public HttpResponseMessage UpdatePriceList(PriceListDto priceListDto) {
            var serviceResult = _commonService.UpdatePriceList(priceListDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getCompanyTerminologies")]
        public HttpResponseMessage GetCompanyTerminologies() {
            var serviceResult = _commonService.GetCompanyTerminologies();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpPost, Route("addCompanyTerminology")]
        public HttpResponseMessage AddCompanyTerminology(CompanyTerminologyDto companyTerminologyDto) {
            var serviceResult = _commonService.AddCompanyTerminology(companyTerminologyDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpPost, Route("editCompanyTerminology")]
        public HttpResponseMessage UpdateCompanyTerminology(CompanyTerminologyDto companyTerminologyDto) {
            var serviceResult = _commonService.UpdateCompanyTerminology(companyTerminologyDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("deleteCompanyTerminology")]
        public HttpResponseMessage DeleteCompanyTerminology(int id) {
            var serviceResult = _commonService.DeleteCompanyTerminology(id);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        
        [HttpGet, Route("getUserRoleTypes")]
        public HttpResponseMessage GetUserRoleTypes() {
            var serviceResult = _commonService.GetUserRoleTypes();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getCountries")]
        public HttpResponseMessage GetCountries() {
            var serviceResult = _commonService.GetCountries();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getCitiesByCountryId")]
        public HttpResponseMessage GetCitiesByCountryId([FromUri]int countryId) {
            var serviceResult = _commonService.GetCitiesByCountryId(countryId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getDistrictByCityId")]
        public HttpResponseMessage GetDistrictByCityId([FromUri]int cityId) {
            var serviceResult = _commonService.GetDistrictByCityId(cityId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getTongues")]
        public HttpResponseMessage GetTongues() {
            var serviceResult = _commonService.GetTongues();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getSpecializations")]
        public HttpResponseMessage GetSpecializations() {
            var serviceResult = _commonService.GetSpecializations();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getSoftwares")]
        public HttpResponseMessage GetSoftwares() {
            var serviceResult = _commonService.GetSoftwares();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getBankAccountTypes")]
        public HttpResponseMessage GetBankAccountTypes() {
            var serviceResult = _commonService.GetBankAccountTypes();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getCurrencies")]
        public HttpResponseMessage GetCurrencies() {
            var serviceResult = _commonService.GetCurrencies();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getWorkingTypes")]
        public HttpResponseMessage GetWorkingTypes() {
            var serviceResult = _commonService.GetWorkingTypes();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getServiceTypes")]
        public HttpResponseMessage GetServiceTypes() {
            var serviceResult = _commonService.GetServiceTypes();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getCompany")]
        public HttpResponseMessage GetCompany(int id) {
            var serviceResult = _commonService.GetCompany(id);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getLanguage")]
        public HttpResponseMessage GetLanguage(int id) {
            var serviceResult = _commonService.GetLanguage(id);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getPriceList")]
        public HttpResponseMessage GetPriceList(int id) {
            var serviceResult = _commonService.GetPriceList(id);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }
        [HttpGet, Route("getCompanyTerminology")]
        public HttpResponseMessage GetCompanyTerminology(int id) {
            var serviceResult = _commonService.GetCompanyTerminology(id);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpGet, Route("getTranslationQualities")]
        public HttpResponseMessage GetTranslationQualities() {
            var serviceResult = _commonService.GetTranslationQualities();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

    }
}
