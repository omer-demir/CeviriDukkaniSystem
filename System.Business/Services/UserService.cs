using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using log4net;
using Tangent.CeviriDukkani.Data.Model;
using Tangent.CeviriDukkani.Domain.Common;
using Tangent.CeviriDukkani.Domain.Dto.Common;
using Tangent.CeviriDukkani.Domain.Dto.Enums;
using Tangent.CeviriDukkani.Domain.Dto.System;
using Tangent.CeviriDukkani.Domain.Entities.Common;
using Tangent.CeviriDukkani.Domain.Entities.System;
using Tangent.CeviriDukkani.Domain.Exceptions;
using Tangent.CeviriDukkani.Domain.Exceptions.ExceptionCodes;
using Tangent.CeviriDukkani.Domain.Mappers;
using EntityState = System.Data.Entity.EntityState;

namespace System.Business.Services {
    public class UserService : IUserService {
        private readonly CeviriDukkaniModel _model;
        private readonly CustomMapperConfiguration _customMapperConfiguration;
        private readonly ILog _logger;
        //private readonly IMailService _mailService;

        public UserService(CeviriDukkaniModel model, CustomMapperConfiguration customMapperConfiguration, ILog logger) {
            _model = model;
            _customMapperConfiguration = customMapperConfiguration;
            _logger = logger;
            //_mailService = new YandexMailService();
        }

        public ServiceResult<UserDto> AddUser(UserDto userDto, int createdBy) {
            var serviceResult = new ServiceResult<UserDto>();
            try {
                userDto.CreatedBy = createdBy;
                userDto.Active = true;

                var user = _customMapperConfiguration.GetMapEntity<User, UserDto>(userDto);

                ////////////////


                #region Email Control

                var tempUser = _model.Users.FirstOrDefault(m => m.Email == user.Email && m.Active);
                if (tempUser != null) {
                    serviceResult.ServiceResultType = ServiceResultType.Warning;
                    serviceResult.Message = ServiceMessage.EmailIsUsed;
                    return serviceResult;
                }

                userDto.UserRoles.ForEach(f => {
                    f.Active = true;
                    f.CreatedBy = createdBy;
                });

                #endregion                

                _model.Users.Add(user);
                //if (userDto.UserRoles?.Count > 0)
                //{
                //    foreach (UserRoleDto userRoleDto in userDto.UserRoles)
                //    {
                //        userRoleDto.CreatedBy = createdBy;
                //        userRoleDto.Active = true;
                //        userRoleDto.UserId = user.Id;

                //        var userRole = _customMapperConfiguration.GetMapEntity<UserRole, UserRoleDto>(userRoleDto);
                //        _model.UserRoles.Add(userRole);
                //    }
                //}
                if (_model.SaveChanges() <= 0) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<UserDto, User>(user);

                //var message = ServiceHelpers.GenerateEmail(user.Name,
                //    user.Email,
                //    user.Password,
                //    MailType.Welcome,
                //    user.Id);

                //var res = _mailService.SendMail("Hoşgeldiniz", message, new string[] { user.Email });
                //if (res.ServiceResultType != ServiceResultType.Success) {
                //    serviceResult.Message = "Email couldn't send to user";
                //    serviceResult.ExceptionCode = ExceptionCodes.EmailCouldntSendToUser;
                //    throw res.Exception;
                //}

            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }

        public ServiceResult<UserDto> EditUser(UserDto userDto, int createdBy) {
            var serviceResult = new ServiceResult<UserDto>();
            try {

                var user = _model.Users.Include(a => a.UserRoles).FirstOrDefault(f => f.Id == userDto.Id);
                if (user == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }
                user.BirthDate = userDto.BirthDate;
                user.Email = userDto.Email;
                user.GenderId = userDto.GenderId;
                user.MobilePhone = userDto.MobilePhone;
                user.Name = userDto.Name;
                user.Password = userDto.Password;
                user.SurName = userDto.SurName;
                if (userDto.UserRoles?.Count > 0) {
                    if (user.UserRoles?.Count > 0) {
                        _model.UserRoles.RemoveRange(user.UserRoles);
                    }

                    foreach (UserRoleDto userRoleDto in userDto.UserRoles) {
                        var userRole = new UserRole {
                            Active = true,
                            CreatedBy = createdBy,
                            UserId = user.Id,
                            UserRoleTypeId = userRoleDto.UserRoleTypeId
                        };
                        //userRoleDto.CreatedBy = createdBy;
                        //userRoleDto.Active = true;
                        //userRoleDto.UserId = user.Id;

                        //var userRole = _customMapperConfiguration.GetMapEntity<UserRole, UserRoleDto>(userRoleDto);
                        _model.UserRoles.Add(userRole);
                    }
                }
                //user.UserRoleType = userDto.UserRoleType;

                userDto.UpdatedBy = createdBy;
                userDto.UpdatedAt = DateTime.Now;

                _model.SaveChanges();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<UserDto, User>(user);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }

        public ServiceResult<UserDto> GetUser(int userId) {
            var serviceResult = new ServiceResult<UserDto>();
            try {
                var user = _model.Users
                    .Include(a => a.UserRoles)
                    .Include(a => a.Gender)
                    .Include(a => a.UserAbility.Capacity)
                    .Include(a => a.UserAbility.Specializations)
                    .Include(a => a.UserAbility.TechnologyKnowledges)
                    .Include(a => a.UserContact.District.City.Country)
                    .Include(a => a.UserPayment.BankAccount)
                    .Include(a => a.UserRate)
                    .Include(a => a.UserScore.UserScoreTransactions.Select(x => x.EditingScore))
                    .Include(a => a.UserScore.UserScoreTransactions.Select(x => x.TranslatingScore))
                    .Include(a => a.UserScore.UserScoreTransactions.Select(x => x.ProofReadingScore))
                    .Include(a => a.UserRoles.Select(x => x.UserRoleType))
                    //.Include("UserAbility.TechnologyKnowledges.Software")
                    .FirstOrDefault(f => f.Id == userId);
                if (user == null) {
                    throw new DbOperationException(ExceptionCodes.NoRelatedData);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<UserDto, User>(user);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }

        public ServiceResult<List<UserDto>> GetUsers() {
            var serviceResult = new ServiceResult<List<UserDto>>();
            try {
                var users = _model.Users
                    .Include(a => a.UserRoles)
                    .Include("UserRoles.UserRoleType")
                    .ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = users.Select(s => _customMapperConfiguration.GetMapDto<UserDto, User>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }

        public ServiceResult<List<UserDto>> GetUsersByUserRoleTypes(List<int> userRoleTypeEnums) {
            var serviceResult = new ServiceResult<List<UserDto>>();
            try {
                var users = _model.Users
                    .Include(a => a.UserRoles)
                    .Include("UserRoles.UserRoleType")
                    .Where(w => w.UserRoles.Any(a => userRoleTypeEnums.Contains(a.UserRoleTypeId)))
                    .ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = users.Select(s => _customMapperConfiguration.GetMapDto<UserDto, User>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }

        public ServiceResult<List<UserDto>> GetTranslatorsAccordingToOrderTranslationQuality(int orderId) {
            var serviceResult = new ServiceResult<List<UserDto>>();
            try {
                var order = _model.Orders.FirstOrDefault(a => a.Id == orderId);
                if (order == null) {
                    throw new BusinessException(ExceptionCodes.NoOrderWithSpecifiedId);
                }

                //1-3 -> standart
                //3-4 -> premium
                //4-5 -> platinium

                var upperLimit = order.TranslationQualityId == (int)TranslationQualityEnum.Standard ? 3 : order.TranslationQualityId == (int)TranslationQualityEnum.Premium ? 4 : 5;
                var lowerLimit = order.TranslationQualityId == (int)TranslationQualityEnum.Standard ? 1 : order.TranslationQualityId == (int)TranslationQualityEnum.Premium ? 3 : 4;

                var translators = (from ur in _model.UserRoles
                                   join us in _model.Users on ur.UserId equals us.Id
                                   join urt in _model.UserRoleTypes on ur.UserRoleTypeId equals urt.Id
                                   join uss in _model.UserScores on us.UserScoreId equals uss.Id
                                   where (ur.UserRoleTypeId == 1 || ur.UserRoleTypeId == 5)
                                         && uss.AverageTranslatingScore >= lowerLimit && uss.AverageTranslatingScore <= upperLimit
                                   select us).Distinct().ToList();
                if (!translators.Any()) {
                    throw new BusinessException("321");
                }


                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = translators.Select(s => _customMapperConfiguration.GetMapDto<UserDto, User>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;

        }

        public ServiceResult<List<UserDto>> GetEditorsAccordingToOrderTranslationQuality(int orderId) {
            var serviceResult = new ServiceResult<List<UserDto>>();
            try {
                var order = _model.Orders.FirstOrDefault(a => a.Id == orderId);
                if (order == null) {
                    throw new BusinessException(ExceptionCodes.NoOrderWithSpecifiedId);
                }

                //1-3 -> standart
                //3-4 -> premium
                //4-5 -> platinium

                var upperLimit = order.TranslationQualityId == (int)TranslationQualityEnum.Standard ? 3 : order.TranslationQualityId == (int)TranslationQualityEnum.Premium ? 4 : 5;
                var lowerLimit = order.TranslationQualityId == (int)TranslationQualityEnum.Standard ? 1 : order.TranslationQualityId == (int)TranslationQualityEnum.Premium ? 3 : 4;

                var translators = (from ur in _model.UserRoles
                                   join us in _model.Users on ur.UserId equals us.Id
                                   join urt in _model.UserRoleTypes on ur.UserRoleTypeId equals urt.Id
                                   join uss in _model.UserScores on us.UserScoreId equals uss.Id
                                   where (ur.UserRoleTypeId == 2)
                                         && uss.AverageTranslatingScore >= lowerLimit && uss.AverageTranslatingScore <= upperLimit
                                   select us).Distinct().ToList();
                if (!translators.Any()) {
                    throw new BusinessException("322");
                }


                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = translators.Select(s => _customMapperConfiguration.GetMapDto<UserDto, User>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }

        public ServiceResult<List<UserDto>> GetProofReadersAccordingToOrderTranslationQuality(int orderId) {
            var serviceResult = new ServiceResult<List<UserDto>>();
            try {
                var order = _model.Orders.FirstOrDefault(a => a.Id == orderId);
                if (order == null) {
                    throw new BusinessException(ExceptionCodes.NoOrderWithSpecifiedId);
                }

                //1-3 -> standart
                //3-4 -> premium
                //4-5 -> platinium

                var upperLimit = order.TranslationQualityId == (int)TranslationQualityEnum.Standard ? 3 : order.TranslationQualityId == (int)TranslationQualityEnum.Premium ? 4 : 5;
                var lowerLimit = order.TranslationQualityId == (int)TranslationQualityEnum.Standard ? 1 : order.TranslationQualityId == (int)TranslationQualityEnum.Premium ? 3 : 4;

                var translators = (from ur in _model.UserRoles
                                   join us in _model.Users on ur.UserId equals us.Id
                                   join urt in _model.UserRoleTypes on ur.UserRoleTypeId equals urt.Id
                                   join uss in _model.UserScores on us.UserScoreId equals uss.Id
                                   where (ur.UserRoleTypeId == 4)
                                         && uss.AverageTranslatingScore >= lowerLimit && uss.AverageTranslatingScore <= upperLimit
                                   select us).Distinct().ToList();
                if (!translators.Any()) {
                    throw new BusinessException("323");
                }


                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = translators.Select(s => _customMapperConfiguration.GetMapDto<UserDto, User>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }

        public ServiceResult CreateUser(UserDto user) {
            var serviceResult = new ServiceResult(ServiceResultType.NotKnown);
            try {
                var entity = _customMapperConfiguration.GetMapEntity<User, UserDto>(user);
                _model.Users.Add(entity);

                serviceResult.Data = _model.SaveChanges() > 0;
                serviceResult.ServiceResultType = ServiceResultType.Success;
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
            }
            return serviceResult;
        }

        public ServiceResult<UserDto> AddOrUpdateUserContact(UserDto userDto, int createdBy) {
            var serviceResult = new ServiceResult<UserDto>();
            try {
                if (userDto?.UserContact == null) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                var user = _model.Users.FirstOrDefault(f => f.Id == userDto.Id);
                if (user == null) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                if (userDto.UserContact.Id == default(int)) {
                    userDto.UserContact.CreatedBy = createdBy;
                    userDto.UserContact.Active = true;
                    var userContact =
                        _customMapperConfiguration.GetMapEntity<UserContact, UserContactDto>(userDto.UserContact);
                    _model.UserContacts.Add(userContact);

                    user.UserContactId = userContact.Id;
                } else {
                    var userContact = _model.UserContacts.FirstOrDefault(f => f.Id == userDto.UserContact.Id);
                    if (userContact == null) {
                        throw new BusinessException(ExceptionCodes.UnableToInsert);
                    }
                    userContact.Address = userDto.UserContact.Address;
                    userContact.AlternativeEmail = userDto.UserContact.AlternativeEmail;
                    userContact.AlternativePhone1 = userDto.UserContact.AlternativePhone1;
                    userContact.AlternativePhone2 = userDto.UserContact.AlternativePhone2;
                    userContact.DistrictId = userDto.UserContact.DistrictId;
                    userContact.Fax = userDto.UserContact.Fax;
                    userContact.PostalCode = userDto.UserContact.PostalCode;
                    userContact.Skype = userDto.UserContact.Skype;

                    userContact.UpdatedAt = DateTime.Now;
                    userContact.UpdatedBy = createdBy;

                }

                if (_model.SaveChanges() <= 0) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<UserDto, User>(user);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }

        public ServiceResult<UserDto> AddOrUpdateUserAbility(UserDto userDto, int createdBy) {
            var serviceResult = new ServiceResult<UserDto>();
            try {
                if (userDto?.UserAbility == null) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                var user = _model.Users.Include(a => a.UserAbility.Specializations).Include(a => a.UserAbility.TechnologyKnowledges).FirstOrDefault(f => f.Id == userDto.Id);
                if (user == null) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                if (userDto.UserAbility.Id == default(int)) {
                    userDto.UserAbility.CreatedBy = createdBy;
                    userDto.UserAbility.Active = true;
                    var userAbility =
                        _customMapperConfiguration.GetMapEntity<UserAbility, UserAbilityDto>(userDto.UserAbility);
                    _model.UserAbilities.Add(userAbility);

                    user.UserAbilityId = userAbility.Id;
                } else {
                    var userAbility = _model.UserAbilities.FirstOrDefault(f => f.Id == userDto.UserAbility.Id);
                    if (userAbility == null) {
                        throw new BusinessException(ExceptionCodes.UnableToInsert);
                    }
                    userAbility.BilingualTongueId = userDto.UserAbility.BilingualTongueId;
                    userAbility.CapacityId = userDto.UserAbility.CapacityId;
                    userAbility.MainClients = userDto.UserAbility.MainClients;
                    userAbility.MotherTongueId = userDto.UserAbility.MotherTongueId;
                    userAbility.Qualifications = userDto.UserAbility.Qualifications;
                    userAbility.YearsOfExperience = userDto.UserAbility.YearsOfExperience;
                    userAbility.QualityEnsureDescription = userDto.UserAbility.QualityEnsureDescription;

                    userAbility.UpdatedAt = DateTime.Now;
                    userAbility.UpdatedBy = createdBy;
                    int capacityId = userDto.UserAbility.CapacityId;
                    if (capacityId == default(int)) // Capacity eklenmemişse ekle
                    {
                        userDto.UserAbility.Capacity.CreatedBy = createdBy;
                        userDto.UserAbility.Capacity.Active = true;

                        if (userDto.UserAbility.Capacity != null) {
                            userDto.UserAbility.Capacity.Active = true;
                            userDto.UserAbility.Capacity.CreatedBy = createdBy;
                        }

                        userDto.UserAbility.TechnologyKnowledges?.ForEach(f => {
                            f.Active = true;
                            f.CreatedBy = createdBy;
                        });

                        userDto.UserAbility.Specializations?.ForEach(f => {
                            f.Active = true;
                            f.CreatedBy = createdBy;
                        });

                        var capacityEntity =
                            _customMapperConfiguration.GetMapEntity<Capacity, CapacityDto>(userDto.UserAbility.Capacity);
                        _model.Capacities.Add(capacityEntity);
                        userDto.UserAbility.CapacityId = capacityEntity.Id;
                    } else {
                        var capacity = _model.Capacities.FirstOrDefault(f => f.Id == userDto.UserAbility.CapacityId);
                        if (capacity == null) {
                            throw new BusinessException(ExceptionCodes.UnableToInsert);
                        }
                        capacity.ProofReading = userDto.UserAbility.Capacity.ProofReading;
                        capacity.Reviews = userDto.UserAbility.Capacity.Reviews;
                        capacity.Translation = userDto.UserAbility.Capacity.Translation;

                        capacity.UpdatedAt = DateTime.Now;
                        capacity.UpdatedBy = createdBy;
                    }

                    if (userDto.UserAbility.Specializations?.Count > 0) {
                        if (user.UserAbility.Specializations?.Count > 0) {
                            _model.Specializations.RemoveRange(user.UserAbility.Specializations);
                        }

                        foreach (SpecializationDto specializationDto in userDto.UserAbility.Specializations) {
                            var specialization = new Specialization {
                                Active = true,
                                CreatedBy = createdBy,
                                UserAbilityId = userDto.UserAbility.Id,
                                TerminologyId = specializationDto.TerminologyId
                            };

                            _model.Specializations.Add(specialization);
                        }
                    }

                    if (userDto.UserAbility.TechnologyKnowledges?.Count > 0) {
                        if (user.UserAbility.TechnologyKnowledges?.Count > 0) {
                            user.UserAbility.TechnologyKnowledges.ForEach(f => {
                                var technologyKnowledgeDto = userDto.UserAbility.TechnologyKnowledges.FirstOrDefault(a => a.Id == f.Id);
                                if (technologyKnowledgeDto == null)
                                    _model.Entry(f).State = EntityState.Deleted;
                            });
                        }
                        foreach (TechnologyKnowledgeDto technologyKnowledgeDto in userDto.UserAbility.TechnologyKnowledges) {
                            if (technologyKnowledgeDto.Id == default(int)) {
                                technologyKnowledgeDto.CreatedBy = createdBy;
                                technologyKnowledgeDto.Active = true;
                                technologyKnowledgeDto.Software = null;
                                var technologyKnowledgeEntity =
                                    _customMapperConfiguration.GetMapEntity<TechnologyKnowledge, TechnologyKnowledgeDto>
                                        (technologyKnowledgeDto);
                                _model.TechnologyKnowledges.Add(technologyKnowledgeEntity);
                            } else {
                                var technologyKnowledge =
                                    user.UserAbility.TechnologyKnowledges?.FirstOrDefault(
                                        f => f.Id == technologyKnowledgeDto.Id);
                                if (technologyKnowledge != null) {
                                    technologyKnowledge.OperatingSystem = technologyKnowledgeDto.OperatingSystem;
                                    technologyKnowledge.SoftwareId = technologyKnowledgeDto.SoftwareId;
                                    technologyKnowledge.SoftwareVersion = technologyKnowledgeDto.SoftwareVersion;
                                    technologyKnowledge.Rating = technologyKnowledgeDto.Rating;
                                }
                            }
                        }
                    } else {
                        _model.TechnologyKnowledges.RemoveRange(user.UserAbility.TechnologyKnowledges);
                    }

                }

                if (_model.SaveChanges() <= 0) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<UserDto, User>(user);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }

        public ServiceResult<UserDto> AddOrUpdateUserPayment(UserDto userDto, int createdBy) {
            var serviceResult = new ServiceResult<UserDto>();
            try {
                if (userDto?.UserPayment == null) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                var user = _model.Users.FirstOrDefault(f => f.Id == userDto.Id);
                if (user == null) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                if (userDto.UserPayment.Id == default(int)) {
                    userDto.UserPayment.CreatedBy = createdBy;
                    userDto.UserPayment.Active = true;

                    if (userDto.UserPayment.BankAccount != null) {
                        userDto.UserPayment.BankAccount.Active = true;
                        userDto.UserPayment.BankAccount.CreatedBy = createdBy;
                    }

                    var userPayment =
                        _customMapperConfiguration.GetMapEntity<UserPayment, UserPaymentDto>(userDto.UserPayment);
                    _model.UserPayments.Add(userPayment);

                    user.UserPaymentId = userPayment.Id;
                } else {
                    var userPayment = _model.UserPayments.FirstOrDefault(f => f.Id == userDto.UserPayment.Id);
                    if (userPayment == null) {
                        throw new BusinessException(ExceptionCodes.UnableToInsert);
                    }

                    var account = _model.BankAccounts.FirstOrDefault(f => f.Id == userDto.UserPayment.BankAccountId);
                    if (account != null) {
                        account.AccountHolderFullName = userDto.UserPayment.BankAccount.AccountHolderFullName;
                        account.AccountNumber = userDto.UserPayment.BankAccount.AccountNumber;
                        account.BankAccountTypeId = userDto.UserPayment.BankAccount.BankAccountTypeId;
                        account.BankAddress = userDto.UserPayment.BankAccount.BankAddress;
                        account.BankName = userDto.UserPayment.BankAccount.BankName;
                        account.BeneficiaryAddress = userDto.UserPayment.BankAccount.BeneficiaryAddress;
                        account.CityCountryBank = userDto.UserPayment.BankAccount.CityCountryBank;
                        account.IBAN = userDto.UserPayment.BankAccount.IBAN;
                        account.PaypalEmailAddress = userDto.UserPayment.BankAccount.PaypalEmailAddress;
                        account.SwiftBicCode = userDto.UserPayment.BankAccount.SwiftBicCode;
                    }

                    userPayment.BankAccountId = userDto.UserPayment.BankAccountId;
                    userPayment.CurrencyId = userDto.UserPayment.CurrencyId;
                    userPayment.MinimumChargeAmount = userDto.UserPayment.MinimumChargeAmount;
                    userPayment.VatTaxNo = userDto.UserPayment.VatTaxNo;
                    userPayment.WorkingTypeId = userDto.UserPayment.WorkingTypeId;

                    userPayment.UpdatedAt = DateTime.Now;
                    userPayment.UpdatedBy = createdBy;

                }

                if (_model.SaveChanges() <= 0) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<UserDto, User>(user);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }

        public ServiceResult<UserDto> AddOrUpdateUserRate(UserDto userDto, int createdBy) {
            var serviceResult = new ServiceResult<UserDto>();
            try {
                if (userDto?.UserRate == null) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                var user = _model.Users.Include(a => a.UserRate.RateItems).FirstOrDefault(f => f.Id == userDto.Id);
                if (user == null) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                if (userDto.UserRate.Id == default(int)) {
                    userDto.UserRate.CreatedBy = createdBy;
                    userDto.UserRate.Active = true;

                    userDto.UserRate.RateItems?.ForEach(f => {
                        f.ServiceType = null;
                        f.SourceLanguage = null;
                        f.TargetLanguage = null;
                        f.Active = true;
                        f.CreatedBy = createdBy;
                    });

                    userDto.UserRate.UserDocuments?.ForEach(f => {
                        f.Active = true;
                        f.CreatedBy = createdBy;
                    });

                    var userRate = _customMapperConfiguration.GetMapEntity<UserRate, UserRateDto>(userDto.UserRate);
                    _model.UserRates.Add(userRate);

                    user.UserRateId = userRate.Id;
                } else {
                    var userRate = _model.UserRates.FirstOrDefault(f => f.Id == userDto.UserRate.Id);
                    if (userRate == null) {
                        throw new BusinessException(ExceptionCodes.UnableToInsert);
                    }
                    userRate.DtpRate = userDto.UserRate.DtpRate;
                    userRate.GlossaryCreationRate = userDto.UserRate.GlossaryCreationRate;
                    userRate.LinguisticTestingRate = userDto.UserRate.LinguisticTestingRate;
                    userRate.ReviewLqaRate = userDto.UserRate.ReviewLqaRate;
                    userRate.TerminologyExtractionRate = userDto.UserRate.TerminologyExtractionRate;
                    userRate.ReviewSmeRate = userDto.UserRate.ReviewSmeRate;
                    userRate.TranslationMemoryManagementRate = userDto.UserRate.TranslationMemoryManagementRate;

                    userRate.UpdatedAt = DateTime.Now;
                    userRate.UpdatedBy = createdBy;

                    if (userDto.UserRate.RateItems?.Count > 0) {
                        if (user.UserRate.RateItems?.Count > 0) {
                            user.UserRate.RateItems.ForEach(f => {
                                var rateItem = userDto.UserRate.RateItems.FirstOrDefault(a => a.Id == f.Id);
                                if (rateItem == null)
                                    _model.Entry(f).State = EntityState.Deleted;
                            });
                        }
                        foreach (RateItemDto rateItemDto in userDto.UserRate.RateItems) {
                            if (rateItemDto.Id == default(int)) {
                                rateItemDto.CreatedBy = createdBy;
                                rateItemDto.Active = true;

                                rateItemDto.ServiceType = null;
                                rateItemDto.SourceLanguage = null;
                                rateItemDto.TargetLanguage = null;

                                var rateItem =
                                    _customMapperConfiguration.GetMapEntity<RateItem, RateItemDto>
                                        (rateItemDto);
                                _model.RateItems.Add(rateItem);
                            } else {
                                var rateItem =
                                    user.UserRate.RateItems?.FirstOrDefault(
                                        f => f.Id == rateItemDto.Id);
                                if (rateItem != null) {
                                    rateItem.CertificateId = rateItemDto.CertificateId;
                                    rateItem.Price = rateItemDto.Price;
                                    rateItem.ServiceTypeId = rateItemDto.ServiceTypeId;
                                    rateItem.SourceLanguageId = rateItemDto.SourceLanguageId;
                                    rateItem.SwornOrCertified = rateItemDto.SwornOrCertified;
                                    rateItem.TargetLanguageId = rateItemDto.TargetLanguageId;

                                }
                            }
                        }
                    } else {
                        _model.RateItems.RemoveRange(user.UserRate.RateItems);
                    }

                }

                if (_model.SaveChanges() <= 0) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<UserDto, User>(user);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }

        public ServiceResult<List<TechnologyKnowledgeDto>> GetTechnologyKnowledgesByUserAbilityId(int userAbilityId) {
            var serviceResult = new ServiceResult<List<TechnologyKnowledgeDto>>();
            try {
                var data = _model.TechnologyKnowledges
                    .Include(a => a.Software)
                    .Where(w => w.UserAbilityId == userAbilityId)
                    .ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = data.Select(s => _customMapperConfiguration.GetMapDto<TechnologyKnowledgeDto, TechnologyKnowledge>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }

        public ServiceResult<List<RateItemDto>> GetRateItemsByUserRateId(int userRateId) {
            var serviceResult = new ServiceResult<List<RateItemDto>>();
            try {
                var data = _model.RateItems
                    .Include(a => a.Certificate)
                    .Include(a => a.ServiceType)
                    .Include(a => a.SourceLanguage)
                    .Include(a => a.TargetLanguage)
                    .Where(w => w.UserRateId == userRateId)
                    .ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = data.Select(s => _customMapperConfiguration.GetMapDto<RateItemDto, RateItem>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
    }
}