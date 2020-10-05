using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

using Hook.Models;
using Hook.Repository;
using Hook.Request;
using Hook.Response;
using Microsoft.Ajax.Utilities;

namespace Hook.Controllers
{
    /// <summary>
    /// Customer Module
    /// </summary>
    public class CustomerController : ApiController
    {
        CustomerRepository customerRepository = new CustomerRepository();

        /// <summary>
        /// Create System User
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(CreateCustomerResult))]
        public IHttpActionResult CreateSystemUser()
        {
            try
            {
                CreateCustomerResult createCustomerResult = customerRepository.CreateSystemUser();

                return Ok(new CreateCustomerResult
                {
                    CreatedSuccessfully = createCustomerResult.CreatedSuccessfully,
                    CustomerId = createCustomerResult.CustomerId,
                    ResponseError = createCustomerResult.ResponseError,
                    StatusCode = (int)HttpStatusCode.OK,
                });
            }
            catch (Exception ex)
            {
                ResponseError responseError = new ResponseError
                {
                    HasError = true,
                    ErrorMessage = ex.Message + (object)"<Br>Inner Exception " + (string)(object)ex.InnerException
                };
                return Ok(new CreateCustomerResult
                {
                    ResponseError = responseError,
                    CreatedSuccessfully = false
                });
            }
        }

        /// <summary>
        /// Create a new Customer here!.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(CreateCustomerResult))]
        public IHttpActionResult CreateCustomer(CreateCustomerRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Customer refResult = customerRepository.GetCustomerByReferalNo(request.RefererRefNo);

                if (refResult != null)
                {
                    CreateCustomerResult createCustomerResult = customerRepository.CreateNewCustomer(request.CustomerTypeId, request.EmailAddress, request.FirstName, request.FullyRegistered, request.IdNumber, request.IdTypeId, request.LanguageId, request.LastName, request.MiddleName, request.RegisteredByUserName, request.IsTestCustomer, request.UserName, request.UserTypeId, long.Parse(request.Msisdn), request.SharedMsisdn, request.RefererRefNo);

                    return Ok(new CreateCustomerResult
                    {
                        CreatedSuccessfully = createCustomerResult.CreatedSuccessfully,
                        CustomerId = createCustomerResult.CustomerId,
                        ResponseError = createCustomerResult.ResponseError,
                        StatusCode = (int)HttpStatusCode.OK,
                    });
                }
                else
                {
                    ResponseError responseError = new ResponseError()
                    {
                        HasError = true,
                        ErrorMessage = "Customer Not Created! Referal No. " + request.RefererRefNo + " Does not Exist!"
                    };
                    return Ok(new CreateCustomerResult
                    {
                        ResponseError = responseError,
                        CreatedSuccessfully = false
                    });
                }
            }
            catch (Exception ex)
            {
                ResponseError responseError = new ResponseError()
                {
                    HasError = true,
                    ErrorMessage = ex.Message + (object)"<Br>Inner Exception " + (string)(object)ex.InnerException
                };
                return Ok(new CreateCustomerResult
                {
                    ResponseError = responseError,
                    CreatedSuccessfully = false
                });
            }
        }

        /// <summary>
        /// Update a Customer Record
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(CustomerDetailsResponse))]
        public IHttpActionResult UpdateCustomer(UpdateCustomerRequest req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                Customer cust = customerRepository.UpdateCustomer(req.CustomerId, req.AccessChannelId, req.CustomerTypeId, req.EmailAddress, req.FirstName, req.FullyRegistered, req.IdNumber, req.IdTypeId, req.InformationModeId, req.IsStaff, req.IsTestCustomer, req.LanguageId, req.LastName, req.MiddleName, req.PostalAddress, req.TaxNumber, long.Parse(req.Msisdn), req.UserTypeId, req.UpdatedBy);

                return Ok(new CustomerDetailsResponse { CustomerId = cust.CustomerId, AccessChannelId = cust.AccessChannelId, BlacklistReasonId = cust.BlacklistReasonId, CreatedDate = cust.CreatedDate, CustomerTypeId = cust.CustomerTypeId, DeactivatedAccount = cust.DeactivatedAccount, DeactivatedDate = cust.DeactivatedDate, DeactivateMsisdns = cust.DeactivateMsisdns, EmailAddress = cust.EmailAddress, FirstName = cust.FirstName, FullyRegistered = cust.FullyRegistered, IdNumber = cust.IdNumber, IdTypeId = cust.IdTypeId, InformationModeId = cust.InformationModeId, IsBlacklisted = cust.IsBlacklisted, IsStaff = cust.IsStaff, IsTestCustomer = cust.IsTestCustomer, LanguageId = cust.LanguageId, LastName = cust.LastName, LoginAttempts = cust.LoginAttempts, MiddleName = cust.MiddleName, Msisdn = cust.Msisdn, PostalAddress = cust.PostalAddress, RegisteredByUsername = cust.RegisteredByUsername, TaxNumber = cust.TaxNumber, TermsAccepted = cust.TermsAccepted, TermsAcceptedDate = cust.TermsAcceptedDate, UserName = cust.UserName, UserTypeId = cust.UserTypeId, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                ResponseError responseError = new ResponseError
                {
                    HasError = true,
                    ErrorMessage = ex.Message + (object)"<Br>Inner Exception " + (string)(object)ex.InnerException
                };
                return Ok(new CreateCustomerResult
                {
                    ResponseError = responseError,
                    CreatedSuccessfully = false
                });
            }
        }

        /// <summary>
        /// Get a Customer by their Username
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(CustomerDetailsResponse))]
        public IHttpActionResult GetCustomerByUsername(CustomerDetailsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                Customer cust = customerRepository.GetCustomerByUsername(request.UserName);

                return Ok(new CustomerDetailsResponse { CustomerId = cust.CustomerId, AccessChannelId = cust.AccessChannelId, BlacklistReasonId = cust.BlacklistReasonId, CreatedDate = cust.CreatedDate, CustomerTypeId = cust.CustomerTypeId, DeactivatedAccount = cust.DeactivatedAccount, DeactivatedDate = cust.DeactivatedDate, DeactivateMsisdns = cust.DeactivateMsisdns, EmailAddress = cust.EmailAddress, FirstName = cust.FirstName, FullyRegistered = cust.FullyRegistered, IdNumber = cust.IdNumber, IdTypeId = cust.IdTypeId, InformationModeId = cust.InformationModeId, IsBlacklisted = cust.IsBlacklisted, IsStaff = cust.IsStaff, IsTestCustomer = cust.IsTestCustomer, LanguageId = cust.LanguageId, LastName = cust.LastName, LoginAttempts = cust.LoginAttempts, MiddleName = cust.MiddleName, Msisdn = cust.Msisdn, PostalAddress = cust.PostalAddress, RegisteredByUsername = cust.RegisteredByUsername, SubCountyId = cust.SubCountyId, TaxNumber = cust.TaxNumber, TermsAccepted = cust.TermsAccepted, TermsAcceptedDate = cust.TermsAcceptedDate, TownId = cust.TownId, UserName = cust.UserName, UserTypeId = cust.UserTypeId, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                ResponseError responseError = new ResponseError
                {
                    HasError = true,
                    ErrorMessage = ex.Message + (object)"<Br>Inner Exception " + (string)(object)ex.InnerException
                };
                return Ok(new CreateCustomerResult
                {
                    ResponseError = responseError,
                    CreatedSuccessfully = false
                });
            }
        }

        /// <summary>
        /// Get a Customer by their Phone Number
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(CustomerDetailsResponse))]
        public IHttpActionResult GetCustomerByMsisdn(CustomerDetailsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                Customer cust = customerRepository.GetCustomerByMsisdn(request.Msisdn);

                return Ok(new CustomerDetailsResponse { CustomerId = cust.CustomerId, AccessChannelId = cust.AccessChannelId, BlacklistReasonId = cust.BlacklistReasonId, CreatedDate = cust.CreatedDate, CustomerTypeId = cust.CustomerTypeId, DeactivatedAccount = cust.DeactivatedAccount, DeactivatedDate = cust.DeactivatedDate, DeactivateMsisdns = cust.DeactivateMsisdns, EmailAddress = cust.EmailAddress, FirstName = cust.FirstName, FullyRegistered = cust.FullyRegistered, IdNumber = cust.IdNumber, IdTypeId = cust.IdTypeId, InformationModeId = cust.InformationModeId, IsBlacklisted = cust.IsBlacklisted, IsStaff = cust.IsStaff, IsTestCustomer = cust.IsTestCustomer, LanguageId = cust.LanguageId, LastName = cust.LastName, LoginAttempts = cust.LoginAttempts, MiddleName = cust.MiddleName, Msisdn = cust.Msisdn, PostalAddress = cust.PostalAddress, RegisteredByUsername = cust.RegisteredByUsername, SubCountyId = cust.SubCountyId, TaxNumber = cust.TaxNumber, TermsAccepted = cust.TermsAccepted, TermsAcceptedDate = cust.TermsAcceptedDate, TownId = cust.TownId, UserName = cust.UserName, UserTypeId = cust.UserTypeId, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });

            }
            catch (Exception ex)
            {
                ResponseError responseError = new ResponseError
                {
                    HasError = true,
                    ErrorMessage = ex.Message + (object)"<Br>Inner Exception " + (string)(object)ex.InnerException
                };
                return Ok(new CreateCustomerResult
                {
                    ResponseError = responseError,
                    CreatedSuccessfully = false
                });
            }
        }

        /// <summary>
        /// Get Customer By Id Number
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(CustomerDetailsResponse))]
        public IHttpActionResult GetCustomerByIdNumber(CustomerDetailsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                Customer cust = customerRepository.GetCustomerByIdNumber(request.IdNumber);

                return Ok(new CustomerDetailsResponse { CustomerId = cust.CustomerId, AccessChannelId = cust.AccessChannelId, BlacklistReasonId = cust.BlacklistReasonId, CreatedDate = cust.CreatedDate, CustomerTypeId = cust.CustomerTypeId, DeactivatedAccount = cust.DeactivatedAccount, DeactivatedDate = cust.DeactivatedDate, DeactivateMsisdns = cust.DeactivateMsisdns, EmailAddress = cust.EmailAddress, FirstName = cust.FirstName, FullyRegistered = cust.FullyRegistered, IdNumber = cust.IdNumber, IdTypeId = cust.IdTypeId, InformationModeId = cust.InformationModeId, IsBlacklisted = cust.IsBlacklisted, IsStaff = cust.IsStaff, IsTestCustomer = cust.IsTestCustomer, LanguageId = cust.LanguageId, LastName = cust.LastName, LoginAttempts = cust.LoginAttempts, MiddleName = cust.MiddleName, Msisdn = cust.Msisdn, PostalAddress = cust.PostalAddress, RegisteredByUsername = cust.RegisteredByUsername, SubCountyId = cust.SubCountyId, TaxNumber = cust.TaxNumber, TermsAccepted = cust.TermsAccepted, TermsAcceptedDate = cust.TermsAcceptedDate, TownId = cust.TownId, UserName = cust.UserName, UserTypeId = cust.UserTypeId, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });

            }
            catch (Exception ex)
            {
                ResponseError responseError = new ResponseError
                {
                    HasError = true,
                    ErrorMessage = ex.Message + (object)"<Br>Inner Exception " + (string)(object)ex.InnerException
                };
                return Ok(new CreateCustomerResult
                {
                    ResponseError = responseError,
                    CreatedSuccessfully = false
                });
            }
        }

        /// <summary>
        /// Get a Customer by their Customer Id
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(CustomerDetailsResponse))]
        public IHttpActionResult GetCustomerByCustomerId(CustomerDetailsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                Customer cust = customerRepository.GetCustomerByCustomerId(request.CustomerId);

                return Ok(new CustomerDetailsResponse { CustomerId = cust.CustomerId, AccessChannelId = cust.AccessChannelId, BlacklistReasonId = cust.BlacklistReasonId, CreatedDate = cust.CreatedDate, CustomerTypeId = cust.CustomerTypeId, DeactivatedAccount = cust.DeactivatedAccount, DeactivatedDate = cust.DeactivatedDate, DeactivateMsisdns = cust.DeactivateMsisdns, EmailAddress = cust.EmailAddress, FirstName = cust.FirstName, FullyRegistered = cust.FullyRegistered, IdNumber = cust.IdNumber, IdTypeId = cust.IdTypeId, InformationModeId = cust.InformationModeId, IsBlacklisted = cust.IsBlacklisted, IsStaff = cust.IsStaff, IsTestCustomer = cust.IsTestCustomer, LanguageId = cust.LanguageId, LastName = cust.LastName, LoginAttempts = cust.LoginAttempts, MiddleName = cust.MiddleName, Msisdn = cust.Msisdn, PostalAddress = cust.PostalAddress, RegisteredByUsername = cust.RegisteredByUsername, SubCountyId = cust.SubCountyId, TaxNumber = cust.TaxNumber, TermsAccepted = cust.TermsAccepted, TermsAcceptedDate = cust.TermsAcceptedDate, TownId = cust.TownId, UserName = cust.UserName, UserTypeId = cust.UserTypeId, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });

            }
            catch (Exception ex)
            {
                ResponseError responseError = new ResponseError
                {
                    HasError = true,
                    ErrorMessage = ex.Message + (object)"<Br>Inner Exception " + (string)(object)ex.InnerException
                };
                return Ok(new CreateCustomerResult
                {
                    ResponseError = responseError,
                    CreatedSuccessfully = false
                });
            }
        }

        /// <summary>
        /// Get all the Customers in the System
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(List<CustomerDetailsResponse>))]
        public IHttpActionResult GetAllCustomers()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                List<CustomerDetailsResponse> response = new List<CustomerDetailsResponse>();

                List<Customer> cust = customerRepository.GetAllCustomers();

                foreach (var item in cust)
                {
                    response.Add(new CustomerDetailsResponse { CustomerId = item.CustomerId, AccessChannelId = item.AccessChannelId, BlacklistReasonId = item.BlacklistReasonId, CreatedDate = item.CreatedDate, CustomerTypeId = item.CustomerTypeId, DeactivatedAccount = item.DeactivatedAccount, DeactivatedDate = item.DeactivatedDate, DeactivateMsisdns = item.DeactivateMsisdns, EmailAddress = item.EmailAddress, FirstName = item.FirstName, FullyRegistered = item.FullyRegistered, IdNumber = item.IdNumber, IdTypeId = item.IdTypeId, InformationModeId = item.InformationModeId, IsBlacklisted = item.IsBlacklisted, IsStaff = item.IsStaff, IsTestCustomer = item.IsTestCustomer, LanguageId = item.LanguageId, LastName = item.LastName, LoginAttempts = item.LoginAttempts, MiddleName = item.MiddleName, Msisdn = item.Msisdn, PostalAddress = item.PostalAddress, RegisteredByUsername = item.RegisteredByUsername, SubCountyId = item.SubCountyId, TaxNumber = item.TaxNumber, TermsAccepted = item.TermsAccepted, TermsAcceptedDate = item.TermsAcceptedDate, TownId = item.TownId, UserName = item.UserName, UserTypeId = item.UserTypeId, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                ResponseError responseError = new ResponseError
                {
                    HasError = true,
                    ErrorMessage = ex.Message + (object)"<Br>Inner Exception " + (string)(object)ex.InnerException
                };
                return Ok(new CreateCustomerResult
                {
                    ResponseError = responseError,
                    CreatedSuccessfully = false
                });
            }
        }

        /// <summary>
        /// Set Customer PIN
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult SetCustomerPIN(PinRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                bool result = customerRepository.SetCustomerPin(request.CustomerId, request.Pin);

                if (result == true)
                {
                    return Ok(new GenericResponse { IsSuccessful = true, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Ok(new GenericResponse { IsSuccessful = false, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });
                }

            }
            catch (Exception ex)
            {
                return Ok(new GenericResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Customer Login
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult CustomerLogin(CustomerLoginRequest req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int loginAttepts = 0;
                Customer customer = customerRepository.CustomerLogin(req.Msisdn, req.Pin, out loginAttepts);

                if (customer != null)
                {
                    return Ok(new GenericResponse { IsSuccessful = true, Message = loginAttepts.ToString(), StatusCode = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Ok(new GenericResponse { IsSuccessful = false, Message = loginAttepts.ToString(), StatusCode = (int)HttpStatusCode.OK });
                }
            }
            catch (Exception ex)
            {
                return Ok(new CustomerLoginResponse { Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError, IsSuccessful = false });
            }
        }

        /// <summary>
        /// Change Customer Pin
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult ChangePin(ChangePinRequest req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                bool transactionResultDto = customerRepository.ChangeCustomerPIN(req.Msisdn, req.OldPin, req.NewPin);

                return Ok(new GenericResponse { IsSuccessful = transactionResultDto, Message = "Processed Successfully!", StatusCode = (int)HttpStatusCode.OK });

            }
            catch (Exception ex)
            {
                return Ok(new GenericResponse { IsSuccessful = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError });
            }
        }

        /// <summary>
        /// Validate Referal Number
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(GenericResponse))]
        public IHttpActionResult ValidateReferalNumber(ValidateReferalNoRequest req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Customer result = customerRepository.GetCustomerByReferalNo(req.RefererRefNumber);

                if (result != null)
                {
                    return Ok(new GenericResponse { StatusCode = (int)HttpStatusCode.OK, Message = result.RefNo, IsSuccessful = true });
                }
                else
                {
                    return Ok(new GenericResponse { StatusCode = (int)HttpStatusCode.OK, Message = "", IsSuccessful = false });
                }
            }
            catch (Exception ex)
            {
                return Ok(new GenericResponse { Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError, IsSuccessful = false });
            }
        }


    }

}
