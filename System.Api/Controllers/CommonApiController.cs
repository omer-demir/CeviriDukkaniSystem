﻿using System.Business.Services;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Tangent.CeviriDukkani.Domain.Common;
using Tangent.CeviriDukkani.Domain.Dto.Common;
using Tangent.CeviriDukkani.Domain.Dto.Request;
using Tangent.CeviriDukkani.Domain.Dto.Sale;
using Tangent.CeviriDukkani.Domain.Dto.Translation;
using Tangent.CeviriDukkani.WebCore.BaseControllers;

namespace System.Api.Controllers {
    [RoutePrefix("api/commonapi")]
    public class CommonApiController : BaseApiController {
        private readonly ICommonService _commonService;

        public CommonApiController(ICommonService commonService) {
            _commonService = commonService;
        }

        [Route("changePassword"), HttpPost]
        public HttpResponseMessage ChangePassword([FromBody]ChangePasswordRequestDto changePasswordRequest) {
            var response = new HttpResponseMessage();

            ServiceResult serviceResult = _commonService.ChangePassword(changePasswordRequest.Email, changePasswordRequest.OldPassword, changePasswordRequest.NewPassword);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.Forbidden;
                response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
            return response;
        }

        [HttpGet, Route("getCompanies")]
        public HttpResponseMessage GetCompanies() {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.GetCompanies();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpPost, Route("addCompany")]
        public HttpResponseMessage AddCompany(CompanyDto companyDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.AddCompany(companyDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpPost, Route("editCompany")]
        public HttpResponseMessage EditCompany(CompanyDto companyDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.UpdateCompany(companyDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }

        [HttpGet, Route("getLanguages")]
        public HttpResponseMessage GetLanguages() {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.GetLanguages();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpPost, Route("addLanguage")]
        public HttpResponseMessage AddLanguage(LanguageDto languageDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.AddLanguage(languageDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpPost, Route("editLanguage")]
        public HttpResponseMessage EditLanguage(LanguageDto languageDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.UpdateLanguage(languageDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }

        [HttpGet, Route("getTargetLanguages")]
        public HttpResponseMessage GetTargetLanguages([FromUri]int sourceLanguageId) {
            var response = new HttpResponseMessage();
            ServiceResult serviceResult = _commonService.GetTargetLanguages(sourceLanguageId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpPost, Route("addSourceTargetLanguages")]
        public HttpResponseMessage AddSourceTargetLanguages(SourceTargetLanguageDto sourceTargetLanguageDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.AddSourceTargetLanguages(sourceTargetLanguageDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpPost, Route("deleteSourceTargetLanguages")]
        public HttpResponseMessage DeleteSourceTargetLanguages(SourceTargetLanguageDto sourceTargetLanguageDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.DeleteSourceTargetLanguages(sourceTargetLanguageDto);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }

        [HttpGet, Route("getTerminologies")]
        public HttpResponseMessage GetTerminologies() {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.GetTerminologies();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpPost, Route("addTerminology")]
        public HttpResponseMessage AddTerminology(TerminologyDto terminologyDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.AddTerminology(terminologyDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpPost, Route("editTerminology")]
        public HttpResponseMessage EditTerminology(TerminologyDto terminologyDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.UpdateTerminology(terminologyDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }

        [HttpGet, Route("getPriceLists")]
        public HttpResponseMessage GetPriceLists() {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.GetPriceLists();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpPost, Route("addPriceList")]
        public HttpResponseMessage AddPriceList(PriceListDto priceListDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.AddPriceList(priceListDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpPost, Route("editPriceList")]
        public HttpResponseMessage UpdatePriceList(PriceListDto priceListDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.UpdatePriceList(priceListDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }

        [HttpGet, Route("getCompanyTerminologies")]
        public HttpResponseMessage GetCompanyTerminologies() {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.GetCompanyTerminologies();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpPost, Route("addCompanyTerminology")]
        public HttpResponseMessage AddCompanyTerminology(CompanyTerminologyDto companyTerminologyDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.AddCompanyTerminology(companyTerminologyDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpPost, Route("editCompanyTerminology")]
        public HttpResponseMessage UpdateCompanyTerminology(CompanyTerminologyDto companyTerminologyDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.UpdateCompanyTerminology(companyTerminologyDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpGet, Route("deleteCompanyTerminology")]
        public HttpResponseMessage DeleteCompanyTerminology(int id) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.DeleteCompanyTerminology(id);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }

        [HttpPost, Route("addMessage")]
        public HttpResponseMessage AddMessage(MessageDto messageDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.AddMessage(messageDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpGet, Route("getIncomingMessages")]
        public HttpResponseMessage GetIncomingMessages([FromUri]int userId) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.GetIncomingMessages(userId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpGet, Route("getSentMessages")]
        public HttpResponseMessage GetSentMessages([FromUri]int userId) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.GetSentMessages(userId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpGet, Route("getMessage")]
        public HttpResponseMessage GetMessage([FromUri]int messageId) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.GetMessage(messageId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpGet, Route("updateMessageForReadDate")]
        public HttpResponseMessage UpdateMessageForReadDate([FromUri]int messageId) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.UpdateMessageForReadDate(messageId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpGet, Route("deleteSentMessage")]
        public HttpResponseMessage DeleteSentMessage([FromUri]int messageId) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.DeleteSentMessage(messageId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpGet, Route("deleteIncomingMessage")]
        public HttpResponseMessage DeleteIncomingMessage([FromUri]int messageId) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.DeleteIncomingMessage(messageId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }

        [HttpGet, Route("getUserRoleTypes")]
        public HttpResponseMessage GetUserRoleTypes() {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.GetUserRoleTypes();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }

        [HttpGet, Route("getCountries")]
        public HttpResponseMessage GetCountries() {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.GetCountries();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }

        [HttpGet, Route("getCitiesByCountryId")]
        public HttpResponseMessage GetCitiesByCountryId([FromUri]int countryId) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.GetCitiesByCountryId(countryId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpGet, Route("getDistrictByCityId")]
        public HttpResponseMessage GetDistrictByCityId([FromUri]int cityId) {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.GetDistrictByCityId(cityId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }

        [HttpGet, Route("getTongues")]
        public HttpResponseMessage GetTongues() {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.GetTongues();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpGet, Route("getSpecializations")]
        public HttpResponseMessage GetSpecializations() {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.GetSpecializations();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }
        [HttpGet, Route("getSoftwares")]
        public HttpResponseMessage GetSoftwares() {
            var response = new HttpResponseMessage();
            var serviceResult = _commonService.GetSoftwares();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.Data.GetType(), serviceResult.Data, Formatter);
            return response;
        }

    }
}
