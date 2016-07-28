using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using log4net;
using Tangent.CeviriDukkani.Data.Model;
using Tangent.CeviriDukkani.Domain.Common;
using Tangent.CeviriDukkani.Domain.Dto.Common;
using Tangent.CeviriDukkani.Domain.Dto.Sale;
using Tangent.CeviriDukkani.Domain.Dto.System;
using Tangent.CeviriDukkani.Domain.Dto.Translation;
using Tangent.CeviriDukkani.Domain.Entities.Common;
using Tangent.CeviriDukkani.Domain.Entities.Sale;
using Tangent.CeviriDukkani.Domain.Entities.System;
using Tangent.CeviriDukkani.Domain.Entities.Translation;
using Tangent.CeviriDukkani.Domain.Exceptions;
using Tangent.CeviriDukkani.Domain.Exceptions.ExceptionCodes;
using Tangent.CeviriDukkani.Domain.Mappers;

namespace System.Business.Services {
    public class CommonService : ICommonService {
        private readonly CeviriDukkaniModel _ceviriDukkaniModel;
        private readonly ICustomMapperConfiguration _customMapperConfiguration;
        private readonly ILog _logger;

        public CommonService(CeviriDukkaniModel ceviriDukkaniModel, ICustomMapperConfiguration customMapperConfiguration,ILog logger) {
            _ceviriDukkaniModel = ceviriDukkaniModel;
            _customMapperConfiguration = customMapperConfiguration;
            _logger = logger;
        }

        public ServiceResult Login(string email, string password) {
            var serviceResult = new ServiceResult();
            try {
                var passwordRetryCount = int.Parse(ConfigurationManager.AppSettings["PasswordRetryCount"]);
                var user = _ceviriDukkaniModel.Users
                                        .Include(a => a.UserRoles)
                                        .Include(a => a.UserRoles.Select(x => x.UserRoleType))
                                        .FirstOrDefault(w => w.Email == email);
                if (user == null) {
                    serviceResult.Message = $"There is no related user with email {email}";
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }
                if (!user.Active) {
                    serviceResult.Message = $"Passive user {email}";
                    throw new DbOperationException(ExceptionCodes.PassiveUser);
                }
                if (!user.Password.Equals(password)) {
                    user.PasswordRetryCount++;

                    if (user.PasswordRetryCount >= passwordRetryCount) {
                        user.Active = false;
                        _ceviriDukkaniModel.SaveChanges();

                        serviceResult.Message = "User locked out for password retry count";
                        throw new DbOperationException(ExceptionCodes.UserLockedOut);
                    }
                    _ceviriDukkaniModel.SaveChanges();

                    serviceResult.Message = $"Wrong password for user email {email}";
                    serviceResult.Data = passwordRetryCount - user.PasswordRetryCount;
                    throw new DbOperationException(ExceptionCodes.WrongPasswordForUser);
                }

                user.PasswordRetryCount = 0;
                _ceviriDukkaniModel.SaveChanges();

                serviceResult.Data = _customMapperConfiguration.GetMapDto<UserDto, User>(user);
                serviceResult.ServiceResultType = ServiceResultType.Success;
            } catch (DbOperationException exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.ExceptionCode = exc.ExceptionCode;
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.ExceptionCode = ExceptionCodes.ObjectReferenceError;
            }

            return serviceResult;
        }
        public ServiceResult ChangePassword(string email, string oldPassword, string newPassword) {
            var serviceResult = new ServiceResult();
            try {
                var passwordRetryCount = int.Parse(ConfigurationManager.AppSettings["PasswordRetryCount"]);
                var user = _ceviriDukkaniModel.Users.FirstOrDefault(w => w.Email == email && w.Password == oldPassword);
                if (user == null) {
                    serviceResult.Message = $"There is no related user with email {email} and password {oldPassword}";
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }
                user.Password = newPassword;
                user.PasswordRetryCount = 0;
                _ceviriDukkaniModel.SaveChanges();

                serviceResult.Data = _customMapperConfiguration.GetMapDto<UserDto, User>(user);
                serviceResult.ServiceResultType = ServiceResultType.Success;
            } catch (DbOperationException exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.ExceptionCode = exc.ExceptionCode;
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.ExceptionCode = ExceptionCodes.ObjectReferenceError;
            }

            return serviceResult;
        }
        public ServiceResult AddMessage(MessageDto messageDto, int createdBy) {
            var serviceResult = new ServiceResult();
            try {
                messageDto.CreatedBy = createdBy;
                messageDto.Active = true;
                messageDto.FromStatus = true;
                messageDto.ToStatus = true;

                var message = _customMapperConfiguration.GetMapEntity<Message, MessageDto>(messageDto);

                _ceviriDukkaniModel.Messages.Add(message);
                if (_ceviriDukkaniModel.SaveChanges() <= 0) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<MessageDto, Message>(message);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetIncomingMessages(int userId) {
            var serviceResult = new ServiceResult();
            try {
                var messages = _ceviriDukkaniModel.Messages.Where(w => w.ToUserId == userId && w.ToStatus).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = messages.Select(s => _customMapperConfiguration.GetMapDto<MessageDto, Message>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetSentMessages(int userId) {
            var serviceResult = new ServiceResult();
            try {
                var messages = _ceviriDukkaniModel.Messages.Where(w => w.FromUserId == userId && w.FromStatus).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = messages.Select(s => _customMapperConfiguration.GetMapDto<MessageDto, Message>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetMessage(int messageId) {
            var serviceResult = new ServiceResult();
            try {
                var message = _ceviriDukkaniModel.Messages.Find(messageId);
                if (message == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<MessageDto, Message>(message);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult UpdateMessageForReadDate(int messageId) {
            var serviceResult = new ServiceResult();
            try {
                var message = _ceviriDukkaniModel.Messages.FirstOrDefault(w => w.Id == messageId);
                if (message == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }

                message.ReadDate = DateTime.Now;

                _ceviriDukkaniModel.SaveChanges();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<MessageDto, Message>(message);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult DeleteSentMessage(int messageId) {
            var serviceResult = new ServiceResult();
            try {
                var message = _ceviriDukkaniModel.Messages.FirstOrDefault(w => w.Id == messageId);
                if (message == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }

                message.FromStatus = false;

                _ceviriDukkaniModel.SaveChanges();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<MessageDto, Message>(message);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult DeleteIncomingMessage(int messageId) {
            var serviceResult = new ServiceResult();
            try {
                var message = _ceviriDukkaniModel.Messages.FirstOrDefault(w => w.Id == messageId);
                if (message == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }

                message.ToStatus = false;

                _ceviriDukkaniModel.SaveChanges();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<MessageDto, Message>(message);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetCompanies() {
            var serviceResult = new ServiceResult();
            try {
                var companies = _ceviriDukkaniModel.Companies.Where(w => w.Active).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = companies.Select(s => _customMapperConfiguration.GetMapDto<CompanyDto, Company>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult AddCompany(CompanyDto companyDto, int createdBy) {
            var serviceResult = new ServiceResult();
            try {
                companyDto.CreatedBy = createdBy;
                companyDto.Active = true;

                var company = _customMapperConfiguration.GetMapEntity<Company, CompanyDto>(companyDto);

                _ceviriDukkaniModel.Companies.Add(company);
                if (_ceviriDukkaniModel.SaveChanges() <= 0) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<CompanyDto, Company>(company);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult UpdateCompany(CompanyDto companyDto, int createdBy) {
            var serviceResult = new ServiceResult();
            try {

                var company = _ceviriDukkaniModel.Companies.FirstOrDefault(f => f.Id == companyDto.Id);
                if (company == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }
                company.Name = companyDto.Name;
                company.TaxNumber = companyDto.TaxNumber;
                company.TaxOffice = companyDto.TaxOffice;
                company.Phone = companyDto.Phone;
                company.ExtensionNumber = companyDto.ExtensionNumber;
                company.AccountingEmail = companyDto.AccountingEmail;
                company.Address = companyDto.Address;
                company.AuthorizedEmail = companyDto.AuthorizedEmail;
                company.AuthorizedFullName = companyDto.AuthorizedFullName;
                company.AuthorizedMobilePhone = companyDto.AuthorizedMobilePhone;
                company.IsContractPrice = companyDto.IsContractPrice;
                company.IsUsingPo = companyDto.IsUsingPo;

                companyDto.UpdatedBy = createdBy;
                companyDto.UpdatedAt = DateTime.Now;

                _ceviriDukkaniModel.SaveChanges();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<CompanyDto, Company>(company);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetCompany(int companyId) {
            var serviceResult = new ServiceResult();
            try {
                var company = _ceviriDukkaniModel.Companies.Find(companyId);
                if (company == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<CompanyDto, Company>(company);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetLanguages() {
            var serviceResult = new ServiceResult();
            try {
                var languages = _ceviriDukkaniModel.Languages.ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = languages.Select(s => _customMapperConfiguration.GetMapDto<LanguageDto, Language>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult AddLanguage(LanguageDto languageDto, int createdBy) {
            var serviceResult = new ServiceResult();
            try {
                languageDto.CreatedBy = createdBy;
                languageDto.Active = true;

                var language = _customMapperConfiguration.GetMapEntity<Language, LanguageDto>(languageDto);

                _ceviriDukkaniModel.Languages.Add(language);
                if (_ceviriDukkaniModel.SaveChanges() <= 0) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<LanguageDto, Language>(language);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult UpdateLanguage(LanguageDto languageDto, int createdBy) {
            var serviceResult = new ServiceResult();
            try {

                var language = _ceviriDukkaniModel.Languages.FirstOrDefault(f => f.Id == languageDto.Id);
                if (language == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }

                language.UpdatedBy = createdBy;
                language.UpdatedAt = DateTime.Now;
                language.Name = languageDto.Name;

                _ceviriDukkaniModel.SaveChanges();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<LanguageDto, Language>(language);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetLanguage(int languageId) {
            var serviceResult = new ServiceResult();
            try {
                var language = _ceviriDukkaniModel.Languages.Find(languageId);
                if (language == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<LanguageDto, Language>(language);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetTargetLanguages(int sourceLanguageId) {
            var serviceResult = new ServiceResult();
            try {
                var languages = _ceviriDukkaniModel.SourceTargetLanguages.Where(w => w.SourceLanguageId == sourceLanguageId).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = languages.Select(s => _customMapperConfiguration.GetMapDto<SourceTargetLanguageDto, SourceTargetLanguage>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult AddSourceTargetLanguages(SourceTargetLanguageDto sourceTargetLanguageDto, int createdBy) {
            var serviceResult = new ServiceResult();
            try {
                sourceTargetLanguageDto.CreatedBy = createdBy;
                sourceTargetLanguageDto.Active = true;

                var language = _customMapperConfiguration.GetMapEntity<SourceTargetLanguage, SourceTargetLanguageDto>(sourceTargetLanguageDto);

                _ceviriDukkaniModel.SourceTargetLanguages.Add(language);
                if (_ceviriDukkaniModel.SaveChanges() <= 0) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<SourceTargetLanguageDto, SourceTargetLanguage>(language);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult DeleteSourceTargetLanguages(SourceTargetLanguageDto sourceTargetLanguageDto) {
            var serviceResult = new ServiceResult();
            try {
                var sourceTargetLanguage =
                    _ceviriDukkaniModel.SourceTargetLanguages.FirstOrDefault(
                        f =>
                            f.SourceLanguageId == sourceTargetLanguageDto.SourceLanguageId &&
                            f.TargetLanguageId == sourceTargetLanguageDto.TargetLanguageId);

                _ceviriDukkaniModel.SourceTargetLanguages.Remove(sourceTargetLanguage);
                if (_ceviriDukkaniModel.SaveChanges() <= 0) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = true;
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetTerminologies() {
            var serviceResult = new ServiceResult();
            try {
                var languages = _ceviriDukkaniModel.Terminologies.ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = languages.Select(s => _customMapperConfiguration.GetMapDto<TerminologyDto, Terminology>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult AddTerminology(TerminologyDto terminologyDto, int createdBy) {
            var serviceResult = new ServiceResult();
            try {
                terminologyDto.CreatedBy = createdBy;
                terminologyDto.Active = true;

                var terminology = _customMapperConfiguration.GetMapEntity<Terminology, TerminologyDto>(terminologyDto);

                _ceviriDukkaniModel.Terminologies.Add(terminology);
                if (_ceviriDukkaniModel.SaveChanges() <= 0) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<TerminologyDto, Terminology>(terminology);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult UpdateTerminology(TerminologyDto terminologyDto, int createdBy) {
            var serviceResult = new ServiceResult();
            try {

                var terminology = _ceviriDukkaniModel.Terminologies.FirstOrDefault(f => f.Id == terminologyDto.Id);
                if (terminology == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }

                terminology.UpdatedBy = createdBy;
                terminology.UpdatedAt = DateTime.Now;
                terminology.Name = terminologyDto.Name;

                _ceviriDukkaniModel.SaveChanges();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<TerminologyDto, Terminology>(terminology);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetTerminology(int terminologyId) {
            var serviceResult = new ServiceResult();
            try {
                var terminology = _ceviriDukkaniModel.Terminologies.Find(terminologyId);
                if (terminology == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<TerminologyDto, Terminology>(terminology);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetPriceLists() {
            var serviceResult = new ServiceResult();
            try {
                var priceLists = _ceviriDukkaniModel.PriceLists.ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = priceLists.Select(s => _customMapperConfiguration.GetMapDto<PriceListDto, PriceList>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult AddPriceList(PriceListDto priceListDto, int createdBy) {
            var serviceResult = new ServiceResult();
            try {
                priceListDto.CreatedBy = createdBy;
                priceListDto.Active = true;

                var priceList = _customMapperConfiguration.GetMapEntity<PriceList, PriceListDto>(priceListDto);

                _ceviriDukkaniModel.PriceLists.Add(priceList);
                if (_ceviriDukkaniModel.SaveChanges() <= 0) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<PriceListDto, PriceList>(priceList);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult UpdatePriceList(PriceListDto priceListDto, int createdBy) {
            var serviceResult = new ServiceResult();
            try {

                var priceList = _ceviriDukkaniModel.PriceLists.FirstOrDefault(f => f.Id == priceListDto.Id);
                if (priceList == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }

                priceList.UpdatedBy = createdBy;
                priceList.UpdatedAt = DateTime.Now;
                priceList.Char_0_100 = priceListDto.Char_0_100;
                priceList.Char_100_150 = priceListDto.Char_100_150;
                priceList.Char_150_200 = priceListDto.Char_150_200;
                priceList.Char_200_500 = priceListDto.Char_200_500;
                priceList.Char_500_More = priceListDto.Char_500_More;
                priceList.SourceLanguageId = priceListDto.SourceLanguageId;
                priceList.TargetLanguageId = priceListDto.TargetLanguageId;

                _ceviriDukkaniModel.SaveChanges();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<PriceListDto, PriceList>(priceList);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetPriceList(int priceListId) {
            var serviceResult = new ServiceResult();
            try {
                var priceList = _ceviriDukkaniModel.PriceLists.Find(priceListId);
                if (priceList == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<PriceListDto, PriceList>(priceList);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetCompanyTerminologies() {
            var serviceResult = new ServiceResult();
            try {
                var companyTerminologies = _ceviriDukkaniModel.CompanyTerminologies.ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = companyTerminologies.Select(s => _customMapperConfiguration.GetMapDto<CompanyTerminologyDto, CompanyTerminology>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult AddCompanyTerminology(CompanyTerminologyDto companyTerminologyDto, int createdBy) {
            var serviceResult = new ServiceResult();
            try {
                companyTerminologyDto.CreatedBy = createdBy;
                companyTerminologyDto.Active = true;

                var companyTerminology = _customMapperConfiguration.GetMapEntity<CompanyTerminology, CompanyTerminologyDto>(companyTerminologyDto);

                _ceviriDukkaniModel.CompanyTerminologies.Add(companyTerminology);
                if (_ceviriDukkaniModel.SaveChanges() <= 0) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<CompanyTerminologyDto, CompanyTerminology>(companyTerminology);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult UpdateCompanyTerminology(CompanyTerminologyDto companyTerminologyDto, int createdBy) {
            var serviceResult = new ServiceResult();
            try {

                var companyTerminology = _ceviriDukkaniModel.CompanyTerminologies.FirstOrDefault(f => f.Id == companyTerminologyDto.Id);
                if (companyTerminology == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }

                companyTerminology.UpdatedBy = createdBy;
                companyTerminology.UpdatedAt = DateTime.Now;
                companyTerminology.Name = companyTerminologyDto.Name;
                companyTerminology.CompanyId = companyTerminologyDto.CompanyId;
                companyTerminology.FileUrl = companyTerminologyDto.FileUrl;

                _ceviriDukkaniModel.SaveChanges();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<CompanyTerminologyDto, CompanyTerminology>(companyTerminology);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult DeleteCompanyTerminology(int companyTerminologyId) {
            var serviceResult = new ServiceResult();
            try {

                var companyTerminology = _ceviriDukkaniModel.CompanyTerminologies.FirstOrDefault(f => f.Id == companyTerminologyId);
                if (companyTerminology == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }

                _ceviriDukkaniModel.CompanyTerminologies.Remove(companyTerminology);

                _ceviriDukkaniModel.SaveChanges();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<CompanyTerminologyDto, CompanyTerminology>(companyTerminology);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetCompanyTerminology(int companyTerminologyId) {
            var serviceResult = new ServiceResult();
            try {
                var companyTerminology = _ceviriDukkaniModel.CompanyTerminologies.Find(companyTerminologyId);
                if (companyTerminology == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<CompanyTerminologyDto, CompanyTerminology>(companyTerminology);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult<List<UserRoleTypeDto>> GetUserRoleTypes() {
            var serviceResult = new ServiceResult<List<UserRoleTypeDto>>();
            try {
                var userRoleTypes = _ceviriDukkaniModel.UserRoleTypes.ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = userRoleTypes.Select(s => _customMapperConfiguration.GetMapDto<UserRoleTypeDto, UserRoleType>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult<List<CountryDto>> GetCountries() {
            var serviceResult = new ServiceResult<List<CountryDto>>();
            try {
                var countries = _ceviriDukkaniModel.Countries.Where(w => w.Active).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = countries.Select(s => _customMapperConfiguration.GetMapDto<CountryDto, Country>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult<List<CityDto>> GetCitiesByCountryId(int countryId) {
            var serviceResult = new ServiceResult<List<CityDto>>();
            try {
                var cities = _ceviriDukkaniModel.Cities.Where(w => w.CountryId == countryId && w.Active).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = cities.Select(s => _customMapperConfiguration.GetMapDto<CityDto, City>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult<List<DistrictDto>> GetDistrictByCityId(int cityId) {
            var serviceResult = new ServiceResult<List<DistrictDto>>();
            try {
                var districts = _ceviriDukkaniModel.Districts.Where(w => w.CityId == cityId && w.Active).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = districts.Select(s => _customMapperConfiguration.GetMapDto<DistrictDto, District>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetTongues() {
            var serviceResult = new ServiceResult();
            try {
                var data = _ceviriDukkaniModel.Tongues.Where(w => w.Active).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = data.Select(s => _customMapperConfiguration.GetMapDto<TongueDto, Tongue>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetSpecializations() {
            var serviceResult = new ServiceResult();
            try {
                var data = _ceviriDukkaniModel.Specializations.Include(a => a.Terminology).Where(w => w.Active).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = data.Select(s => _customMapperConfiguration.GetMapDto<SpecializationDto, Specialization>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetSoftwares() {
            var serviceResult = new ServiceResult();
            try {
                var data = _ceviriDukkaniModel.Softwares.Where(w => w.Active).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = data.Select(s => _customMapperConfiguration.GetMapDto<SoftwareDto, Software>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetBankAccountTypes() {
            var serviceResult = new ServiceResult();
            try {
                var data = _ceviriDukkaniModel.BankAccountTypes.Where(w => w.Active).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = data.Select(s => _customMapperConfiguration.GetMapDto<BankAccountTypeDto, BankAccountType>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetCurrencies() {
            var serviceResult = new ServiceResult();
            try {
                var data = _ceviriDukkaniModel.Currencies.Where(w => w.Active).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = data.Select(s => _customMapperConfiguration.GetMapDto<CurrencyDto, Currency>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetWorkingTypes() {
            var serviceResult = new ServiceResult();
            try {
                var data = _ceviriDukkaniModel.WorkingTypes.Where(w => w.Active).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = data.Select(s => _customMapperConfiguration.GetMapDto<WorkingTypeDto, WorkingType>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetServiceTypes() {
            var serviceResult = new ServiceResult();
            try {
                var data = _ceviriDukkaniModel.ServiceTypes.Where(w => w.Active).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = data.Select(s => _customMapperConfiguration.GetMapDto<ServiceTypeDto, ServiceType>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
    }
}