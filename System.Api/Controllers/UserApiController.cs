﻿using System.Business.Services;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Tangent.CeviriDukkani.Domain.Common;
using Tangent.CeviriDukkani.Domain.Dto.Request;
using Tangent.CeviriDukkani.Domain.Dto.System;
using Tangent.CeviriDukkani.WebCore.BaseControllers;

namespace System.Api.Controllers
{
    [RoutePrefix("api/userapi")]
    public class UserApiController : BaseApiController {
        private readonly IUserService _userService;

        public UserApiController(IUserService userService) {
            _userService = userService;
        }

        [HttpPost, Route("updateUserRegistration")]
        public HttpResponseMessage UpdateUserRegistration(UpdateUserStepRequestDto request) {
            var serviceResult = _userService.UpdateUserRegistration(request.User,request.Step);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpGet, Route("getUserRegistration")]
        public HttpResponseMessage GetUserRegistration(int userId) {
            var response = new HttpResponseMessage();
            var serviceResult = _userService.GetUserRegistration(userId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
            return response;
        }

        [HttpPost, Route("addUser")]
        public HttpResponseMessage AddUser(UserDto userDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _userService.AddUser(userDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
            return response;
        }

        [HttpPost, Route("editUser")]
        public HttpResponseMessage EditUser(UserDto userDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _userService.EditUser(userDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
            return response;
        }

        [HttpGet, Route("getUsers")]
        public HttpResponseMessage GetUsers() {
            var response = new HttpResponseMessage();
            var serviceResult = _userService.GetUsers();
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
            return response;
        }

        [HttpGet, Route("getUser")]
        public HttpResponseMessage GetUser(int userId)
        {
            var response = new HttpResponseMessage();
            var serviceResult = _userService.GetUser(userId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
            return response;
        }

        [HttpPost, Route("GetUsersByUserRoleTypes")]
        public HttpResponseMessage GetUsersByUserRoleTypes(List<int> userRoleTypeEnums)
        {
            var response = new HttpResponseMessage();
            var serviceResult = _userService.GetUsersByUserRoleTypes(userRoleTypeEnums);
            if (serviceResult.ServiceResultType != ServiceResultType.Success)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }
            //var userDtoList = new List<UserDto> { new UserDto { Name = "Ahmet", Id = 1 } };
            //serviceResult.Data = userDtoList;
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
            return response;
        }

        [HttpPost, Route("editUserContact")]
        public HttpResponseMessage EditUserContact(UserDto userDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _userService.AddOrUpdateUserContact(userDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
            return response;
        }

        [HttpPost, Route("editUserAbility")]
        public HttpResponseMessage EditUserAbility(UserDto userDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _userService.AddOrUpdateUserAbility(userDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
            return response;
        }

        [HttpPost, Route("editUserPayment")]
        public HttpResponseMessage EditUserPayment(UserDto userDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _userService.AddOrUpdateUserPayment(userDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
            return response;
        }

        [HttpPost, Route("editUserRate")]
        public HttpResponseMessage EditUserRate(UserDto userDto) {
            var response = new HttpResponseMessage();
            var serviceResult = _userService.AddOrUpdateUserRate(userDto, 1);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
            return response;
        }

        [HttpGet, Route("getTechnologyKnowledgesByUserAbilityId")]
        public HttpResponseMessage GetTechnologyKnowledgesByUserAbilityId([FromUri]int userAbilityId) {
            var response = new HttpResponseMessage();
            var serviceResult = _userService.GetTechnologyKnowledgesByUserAbilityId(userAbilityId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
            return response;
        }

        [HttpGet, Route("getRateItemsByUserRateId")]
        public HttpResponseMessage GetRateItemsByUserRateId([FromUri]int userRateId) {
            var response = new HttpResponseMessage();
            var serviceResult = _userService.GetRateItemsByUserRateId(userRateId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
            return response;
        }

        [HttpGet, Route("getTranslatorsAccordingToOrderTranslationQuality")]
        public HttpResponseMessage GetTranslatorsAccordingToOrderTranslationQuality([FromUri] int orderId) {
            var serviceResult = _userService.GetTranslatorsAccordingToOrderTranslationQuality(orderId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpGet, Route("getEditorAccordingToOrderTranslationQuality")]
        public HttpResponseMessage GetEditorsAccordingToOrderTranslationQuality([FromUri] int orderId) {
            var serviceResult = _userService.GetEditorsAccordingToOrderTranslationQuality(orderId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpGet, Route("getProofReadersAccordingToOrderTranslationQuality")]
        public HttpResponseMessage GetProofReadersAccordingToOrderTranslationQuality([FromUri] int orderId) {
            var serviceResult = _userService.GetProofReadersAccordingToOrderTranslationQuality(orderId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpPost, Route("setActive")]
        public HttpResponseMessage SetActive(UserDto userDto)
        {
            var response = new HttpResponseMessage();
            var serviceResult = _userService.SetActive(userDto);
            if (serviceResult.ServiceResultType != ServiceResultType.Success)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }


            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
            return response;
        }

    }
}
