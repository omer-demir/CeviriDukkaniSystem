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
        internal ILog Log { get; } = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly CeviriDukkaniModel _model;
        private readonly ICustomMapperConfiguration _customMapperConfiguration;

        public UserService(CeviriDukkaniModel model, ICustomMapperConfiguration customMapperConfiguration) {
            _model = model;
            _customMapperConfiguration = customMapperConfiguration;
        }

        public ServiceResult AddUser(UserDto userDto, int createdBy) {
            var serviceResult = new ServiceResult();
            try {
                userDto.CreatedBy = createdBy;
                userDto.Active = true;

                var user = _customMapperConfiguration.GetMapEntity<User, UserDto>(userDto);

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

                _model.Users.Add(user);

                if (_model.SaveChanges() <= 0) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }
                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<UserDto, User>(user);

            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                Log.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult EditUser(UserDto userDto, int createdBy) {
            var serviceResult = new ServiceResult();
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
                Log.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetUser(int userId) {
            var serviceResult = new ServiceResult();
            try {
                var user = _model.Users
                    .Include(a => a.UserRoles)
                    .Include(a => a.Gender)
                    .Include(a => a.UserAbility.Capacity)
                    .Include(a => a.UserAbility.Specializations)
                    .Include(a => a.UserAbility.TechnologyKnowledges)
                    .Include(a => a.UserContact.District.City.Country)
                    .Include(a => a.UserPayment)
                    .Include(a => a.UserRate)
                    .Include(a => a.UserScore)
                    .Include("UserRoles.UserRoleType")
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
                Log.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetUsers() {
            var serviceResult = new ServiceResult();
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
                Log.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetTranslatorsAccordingToOrderTranslationQuality(int orderId) {

            var serviceResult = new ServiceResult();
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

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = translators.Select(s => _customMapperConfiguration.GetMapDto<UserDto, User>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                Log.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
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
        public ServiceResult AddOrUpdateUserContact(UserDto userDto, int createdBy) {
            var serviceResult = new ServiceResult();
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
                Log.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult AddOrUpdateUserAbility(UserDto userDto, int createdBy) {
            var serviceResult = new ServiceResult();
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
                Log.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult AddOrUpdateUserPayment(UserDto userDto, int createdBy) {
            var serviceResult = new ServiceResult();
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

                    if (userDto.UserPayment.Currency != null) {
                        userDto.UserPayment.Currency.Active = true;
                        userDto.UserPayment.Currency.CreatedBy = createdBy;
                    }

                    if (userDto.UserPayment.WorkingType != null) {
                        userDto.UserPayment.WorkingType.Active = true;
                        userDto.UserPayment.WorkingType.CreatedBy = createdBy;
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
                    userPayment.BankAccountId = userDto.UserPayment.BankAccount;
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
                Log.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult AddOrUpdateUserRate(UserDto userDto, int createdBy) {
            var serviceResult = new ServiceResult();
            try {
                if (userDto?.UserRate == null) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                var user = _model.Users.FirstOrDefault(f => f.Id == userDto.Id);
                if (user == null) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                if (userDto.UserRate.Id == default(int)) {
                    userDto.UserRate.CreatedBy = createdBy;
                    userDto.UserRate.Active = true;
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

                }

                if (_model.SaveChanges() <= 0) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = _customMapperConfiguration.GetMapDto<UserDto, User>(user);
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                Log.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
        public ServiceResult GetTechnologyKnowledgesByUserAbilityId(int userAbilityId) {
            var serviceResult = new ServiceResult();
            try {
                var users = _model.TechnologyKnowledges
                    .Include(a => a.Software)
                    .Where(w => w.UserAbilityId == userAbilityId)
                    .ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = users.Select(s => _customMapperConfiguration.GetMapDto<TechnologyKnowledgeDto, TechnologyKnowledge>(s)).ToList();
            } catch (Exception exc) {
                serviceResult.Exception = exc;
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                Log.Error($"Error occured in {MethodBase.GetCurrentMethod().Name} with exception message {exc.Message} and inner exception {exc.InnerException?.Message}");
            }
            return serviceResult;
        }
    }
}