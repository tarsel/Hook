using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

using Dapper;

using Hook.Enums;
using Hook.Helper;
using Hook.Models;
using Hook.Response;
using Hook.Security;

namespace Hook.Repository
{
    public class CustomerRepository
    {
        string sqlConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString();

        RandomStringGenerator randomStringGenerator = new RandomStringGenerator();
        TransactionRepository transactionRepository = new TransactionRepository();

        public CreateCustomerResult CreateNewCustomer(long customerTypeId, string emailAddress, string firstName, bool fullyRegistered, string idNumber, long idTypeId, long languageId, string lastName, string middleName, string registeredByUsername, bool isTestCustomer, string userName, long userTypeId, long msisdn, bool sharedMsisdn, string refererRefNo)
        {

            try
            {
                long createdCustomerId = 0;

                if (isTestCustomer)//Check if its a test customer for him not to be registered!
                {
                    throw new NotImplementedException("Test Customer");
                }

                Customer currentCustomer = new Customer
                {
                    CustomerTypeId = customerTypeId,
                    RegisteredByUsername = registeredByUsername.ToLower(),
                    UserName = string.IsNullOrEmpty(userName) ? null : userName.ToLower(),
                    CreatedDate = GetRealDate(),
                    FirstName = firstName.ToUpper(),
                    LastName = lastName.ToUpper(),
                    MiddleName = string.IsNullOrEmpty(middleName.ToUpper()) ? null : middleName.ToUpper(),
                    LanguageId = languageId,
                    UserTypeId = userTypeId,
                    FullyRegistered = fullyRegistered,
                    EmailAddress = string.IsNullOrEmpty(emailAddress) ? null : emailAddress,
                    InformationModeId = 1,
                    IsBlacklisted = false,
                    IsTestCustomer = isTestCustomer,
                    IdNumber = idNumber,
                    IdTypeId = idTypeId,
                    AccessChannelId = 1,
                    SecurityCode = string.Empty,
                    LoginAttempts = 0,
                    UserLoggedIn = false,
                    TaxNumber = null,
                    TermsAccepted = false,
                    TermsAcceptedDate = null,
                    DeactivatedAccount = false,
                    Nonce = null,
                    Salt = randomStringGenerator.NextString(256, true, true, true, true),
                    Msisdn = msisdn,
                    IsStaff = false,
                    BlacklistReasonId = 1,
                    RefNo = "HK" + randomStringGenerator.NextString(5, false, true, true, false),
                    RefererRefNo = refererRefNo
                };

                if (userTypeId == (int)UserTypes.Business || userTypeId == (int)UserTypes.Company)
                {
                    string notApplicationnameString = null;

                    currentCustomer.FirstName = string.Format("{0} {1}", firstName.ToUpper(), lastName.ToUpper());
                    currentCustomer.MiddleName = notApplicationnameString;
                    currentCustomer.LastName = notApplicationnameString;
                    currentCustomer.IdTypeId = 4; /*Company ID*/
                    currentCustomer.IdNumber = null;
                    currentCustomer.TaxNumber = idNumber;
                }

                Customer customerMsisdn = GetCustomerByMsisdn(msisdn);


                if (customerMsisdn == null || string.IsNullOrEmpty(customerMsisdn.FirstName))
                {
                    using (var connection = new SqlConnection(sqlConnectionString))
                    {
                        connection.Open();
                        var affectedRows = connection.Execute("Insert into Customer (CustomerTypeId, RegisteredByUsername, UserName, CreatedDate, FirstName, LastName, MiddleName, LanguageId, UserTypeId, FullyRegistered, EmailAddress, InformationModeId, IsBlacklisted, IsTestCustomer, IdNumber, IdTypeId, AccessChannelId, SecurityCode, LoginAttempts, UserLoggedIn, TaxNumber, TermsAccepted, TermsAcceptedDate, DeactivatedAccount, Nonce, Salt, Msisdn,BlacklistReasonId,IsStaff,RefNo,RefererRefNo) values (@CustomerTypeId, @RegisteredByUsername, @UserName, @CreatedDate, @FirstName, @LastName, @MiddleName, @LanguageId, @UserTypeId, @FullyRegistered, @EmailAddress, @InformationModeId, @IsBlacklisted, @IsTestCustomer, @IdNumber, @IdTypeId, @AccessChannelId, @SecurityCode, @LoginAttempts, @UserLoggedIn, @TaxNumber, @TermsAccepted, @TermsAcceptedDate, @DeactivatedAccount, @Nonce, @Salt, @Msisdn,@BlacklistReasonId,@IsStaff,@RefNo,@RefererRefNo)", new { currentCustomer.CustomerTypeId, currentCustomer.RegisteredByUsername, currentCustomer.UserName, currentCustomer.CreatedDate, currentCustomer.FirstName, currentCustomer.LastName, currentCustomer.MiddleName, currentCustomer.LanguageId, currentCustomer.UserTypeId, currentCustomer.FullyRegistered, currentCustomer.EmailAddress, currentCustomer.InformationModeId, currentCustomer.IsBlacklisted, currentCustomer.IsTestCustomer, currentCustomer.IdNumber, currentCustomer.IdTypeId, currentCustomer.AccessChannelId, currentCustomer.SecurityCode, currentCustomer.LoginAttempts, currentCustomer.UserLoggedIn, currentCustomer.TaxNumber, currentCustomer.TermsAccepted, currentCustomer.TermsAcceptedDate, currentCustomer.DeactivatedAccount, currentCustomer.Nonce, currentCustomer.Salt, currentCustomer.Msisdn, currentCustomer.BlacklistReasonId, currentCustomer.IsStaff, currentCustomer.RefNo, currentCustomer.RefererRefNo });

                        connection.Close();

                    }

                    createdCustomerId = GetCustomerByMsisdn(msisdn).CustomerId;
                    PaymentInstrument accountCreated = transactionRepository.CreatePaymentInstrument(createdCustomerId);

                }
                else
                {
                    Customer initialCustomer = customerMsisdn;

                    if (!sharedMsisdn || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(initialCustomer.UserName))
                    {
                        ResponseError error = new ResponseError() { HasError = true, ErrorMessage = "MSISDN already registered. Please specify 'SharedMsisdn' as 'True' and specify The UserName of the original account owner and current user you are registering." };
                        return new CreateCustomerResult { ResponseError = error, CreatedSuccessfully = false, CustomerId = createdCustomerId };
                    }
                    else
                    {
                        using (var connection = new SqlConnection(sqlConnectionString))
                        {
                            connection.Open();
                            var affectedRows = connection.Execute("Insert into Customer (CustomerTypeId, RegisteredByUsername, UserName, CreatedDate, FirstName, LastName, MiddleName, LanguageId, UserTypeId, FullyRegistered, EmailAddress, InformationModeId, IsBlacklisted, IsTestCustomer, IdNumber, IdTypeId, AccessChannelId,  SecurityCode, LoginAttempts, UserLoggedIn, TaxNumber, TermsAccepted, TermsAcceptedDate, DeactivatedAccount, Nonce, Salt, Msisdn,BlacklistReasonId,IsStaff,RefNo,RefererRefNo) values (@CustomerTypeId, @RegisteredByUsername, @UserName, @CreatedDate, @FirstName, @LastName, @MiddleName, @LanguageId, @UserTypeId, @FullyRegistered, @EmailAddress, @InformationModeId, @IsBlacklisted, @IsTestCustomer, @IdNumber, @IdTypeId, @AccessChannelId,  @SecurityCode, @LoginAttempts, @UserLoggedIn, @TaxNumber, @TermsAccepted, @TermsAcceptedDate, @DeactivatedAccount, @Nonce, @Salt, @Msisdn,@BlacklistReasonId,@IsStaff,@RefNo,@RefererRefNo)", new { currentCustomer.CustomerTypeId, currentCustomer.RegisteredByUsername, currentCustomer.UserName, currentCustomer.CreatedDate, currentCustomer.FirstName, currentCustomer.LastName, currentCustomer.MiddleName, currentCustomer.LanguageId, currentCustomer.UserTypeId, currentCustomer.FullyRegistered, currentCustomer.EmailAddress, currentCustomer.InformationModeId, currentCustomer.IsBlacklisted, currentCustomer.IsTestCustomer, currentCustomer.IdNumber, currentCustomer.IdTypeId, currentCustomer.AccessChannelId, currentCustomer.SecurityCode, currentCustomer.LoginAttempts, currentCustomer.UserLoggedIn, currentCustomer.TaxNumber, currentCustomer.TermsAccepted, currentCustomer.TermsAcceptedDate, currentCustomer.DeactivatedAccount, currentCustomer.Nonce, currentCustomer.Salt, currentCustomer.Msisdn, currentCustomer.BlacklistReasonId, currentCustomer.IsStaff, currentCustomer.RefNo, currentCustomer.RefererRefNo });

                            connection.Close();

                            createdCustomerId = GetCustomerByMsisdn(msisdn).CustomerId;
                            PaymentInstrument accountCreated = transactionRepository.CreatePaymentInstrument(createdCustomerId);
                        }
                    }
                }

                if (createdCustomerId > 0)
                {
                    ResponseError error = new ResponseError() { HasError = false, ErrorMessage = string.Empty };
                    return new CreateCustomerResult { ResponseError = error, CreatedSuccessfully = true, CustomerId = createdCustomerId };
                }
                else
                {
                    ResponseError error = new ResponseError() { HasError = true, ErrorMessage = string.Empty };
                    return new CreateCustomerResult { ResponseError = error, CreatedSuccessfully = false, CustomerId = -1 };
                }
            }
            catch (Exception ex)
            {
                ResponseError Error = new ResponseError() { HasError = true, ErrorMessage = string.Format("{0}<Br>Inner Exception {1}", ex.Message, ex.InnerException) };
                return new CreateCustomerResult { ResponseError = Error, CreatedSuccessfully = false };
            }
        }

        public CreateCustomerResult CreateSystemUser()
        {
            try
            {
                string firstName = "Hook";
                string lastName = "Hook";
                string idNumber = null;
                string username = "system";
                long msisdn = 100000000001;

                string salt = randomStringGenerator.NextString(100, true, true, true, true);
                string nonce = randomStringGenerator.NextString(100, true, true, true, true);


                if (!string.IsNullOrEmpty(username))
                {
                    Customer existingcustomer = GetCustomerByUsername(username);

                    if (existingcustomer != null)
                    {
                        throw new Exception(string.Format("The Username '{0}' has already been taken", username));
                    }
                }

                Customer customer = new Customer
                {
                    CustomerTypeId = (int)CustomerTypes.SystemUser,
                    RegisteredByUsername = username.ToLower(),
                    UserName = username.ToLower(),
                    CreatedDate = GetRealDate(),
                    FirstName = firstName.ToUpper(),
                    LastName = lastName.ToUpper(),
                    MiddleName = null,
                    LanguageId = 1,
                    UserTypeId = 1,
                    FullyRegistered = true,
                    EmailAddress = null,
                    InformationModeId = 1,
                    IsBlacklisted = false,
                    IsTestCustomer = false,
                    IdNumber = idNumber,
                    IdTypeId = 1,
                    AccessChannelId = 1,
                    TownId = 1,
                    SubCountyId = 1,
                    LoginAttempts = 0,
                    UserLoggedIn = false,
                    TaxNumber = null,
                    TermsAccepted = false,
                    TermsAcceptedDate = null,
                    DeactivatedAccount = true,
                    Nonce = nonce,
                    Salt = salt,
                    BlacklistReasonId = 1,
                    DeactivatedDate = GetRealDate(),
                    Msisdn = msisdn,
                    SecurityCode = randomStringGenerator.NextString(256, true, true, true, true),
                    IsStaff = true,
                    RefNo = "HK" + randomStringGenerator.NextString(5, false, true, true, false),
                    RefererRefNo = ""

                };


                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    var affectedRows = connection.Execute("Insert into Customer (CustomerTypeId, RegisteredByUsername, UserName, CreatedDate, FirstName, LastName, MiddleName, LanguageId, UserTypeId, FullyRegistered, EmailAddress, InformationModeId, IsBlacklisted, IsTestCustomer, IdNumber, IdTypeId, AccessChannelId, LoginAttempts, UserLoggedIn, TaxNumber, TermsAccepted, TermsAcceptedDate, DeactivatedAccount, Nonce, Salt, Msisdn, SecurityCode,BlacklistReasonId,IsStaff,RefNo,RefererRefNo) values (@CustomerTypeId, @RegisteredByUsername, @UserName, @CreatedDate, @FirstName, @LastName, @MiddleName, @LanguageId, @UserTypeId, @FullyRegistered, @EmailAddress, @InformationModeId, @IsBlacklisted, @IsTestCustomer, @IdNumber, @IdTypeId, @AccessChannelId,  @LoginAttempts, @UserLoggedIn, @TaxNumber, @TermsAccepted, @TermsAcceptedDate, @DeactivatedAccount, @Nonce, @Salt, @Msisdn, @SecurityCode,@BlacklistReasonId,@IsStaff,@RefNo,@RefererRefNo)", new

                    { customer.CustomerTypeId, customer.RegisteredByUsername, customer.UserName, customer.CreatedDate, customer.FirstName, customer.LastName, customer.MiddleName, customer.LanguageId, customer.UserTypeId, customer.FullyRegistered, customer.EmailAddress, customer.InformationModeId, customer.IsBlacklisted, customer.IsTestCustomer, customer.IdNumber, customer.IdTypeId, customer.AccessChannelId, customer.SecurityCode, customer.LoginAttempts, customer.UserLoggedIn, customer.TaxNumber, customer.TermsAccepted, customer.TermsAcceptedDate, customer.DeactivatedAccount, customer.Nonce, customer.Salt, customer.Msisdn, customer.BlacklistReasonId, customer.IsStaff, customer.RefNo, customer.RefererRefNo });

                    connection.Close();
                }

                long systemUserId = GetCustomerByMsisdn(msisdn).CustomerId;

                PaymentInstrument accountCreated = transactionRepository.CreatePaymentInstrument(systemUserId);

                ResponseError Error = new ResponseError() { HasError = false, ErrorMessage = null };
                return new CreateCustomerResult { ResponseError = Error, CreatedSuccessfully = true, CustomerId = systemUserId };

            }
            catch (Exception ex)
            {

                ResponseError Error = new ResponseError() { HasError = true, ErrorMessage = String.Format("{0}<Br>Inner Exception {1}", ex.Message, ex.InnerException) };
                return new CreateCustomerResult { ResponseError = Error, CreatedSuccessfully = false };
            }
        }

        public Customer UpdateCustomer(long customerId, long accessChannelId, long customerTypeId, string emailAddress, string firstName, bool fullyRegistered, string idNumber, long idTypeId, long informationModeId, bool isStaff, bool isTestCustomer, long languageId, string lastName, string middleName, string postalAddress, string taxNumber, long msisdn, long userTypeId, string updatedBy)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("UPDATE Customer SET CustomerTypeId=@CustomerTypeId, FirstName=@FirstName, LastName=@LastName, MiddleName=@MiddleName, LanguageId=@LanguageId, UserTypeId=@UserTypeId, FullyRegistered=@FullyRegistered, EmailAddress=@EmailAddress, InformationModeId=@InformationModeId, IsTestCustomer=@IsTestCustomer, IdNumber=@IdNumber, IdTypeId=@IdTypeId, AccessChannelId=@AccessChannelId, TaxNumber=@TaxNumber, Msisdn=@Msisdn, IsStaff=@IsStaff, PostalAddress=@PostalAddress, UpdatedBy=@UpdatedBy, UpdatedDate=@UpdatedDate WHERE CustomerId=@CustomerId", new { CustomerTypeId = customerTypeId, FirstName = firstName.ToUpper(), LastName = lastName.ToUpper(), MiddleName = middleName.ToUpper(), LanguageId = languageId, UserTypeId = userTypeId, FullyRegistered = fullyRegistered, EmailAddress = emailAddress, InformationModeId = informationModeId, IsTestCustomer = isTestCustomer, IdNumber = idNumber, IdTypeId = idTypeId, AccessChannelId = accessChannelId, TaxNumber = taxNumber, Msisdn = msisdn, IsStaff = isStaff, CustomerId = customerId, PostalAddress = postalAddress, UpdatedBy = updatedBy, UpdatedDate = GetRealDate() });

                connection.Close();
            }

            return GetCustomerByCustomerId(customerId);
        }

        public Customer GetCustomerByCustomerId(long customerId)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                return connection.Query<Customer>("SELECT * FROM Customer WHERE CustomerId=@CustomerId", new { CustomerID = customerId }).SingleOrDefault();
            }
        }

        public Customer GetCustomerByIdNumber(string idNumber)
        {

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                return connection.Query<Customer>("SELECT * FROM Customer WHERE IdNumber=@IdNumber", new { IdNumber = idNumber }).SingleOrDefault();
            }
        }

        public Customer GetCustomerByMsisdn(long msisdn)
        {

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                var customer = connection.Query<Customer>("SELECT * FROM Customer WHERE Msisdn=@Msisdn", new { Msisdn = msisdn }).SingleOrDefault();

                return customer;
            }
        }

        public Customer GetCustomerByUsername(string username)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                return connection.Query<Customer>("SELECT * FROM Customer WHERE UserName=@UserName", new { UserName = username }).SingleOrDefault();
            }
        }

        public List<Customer> GetAllCustomers()
        {
            List<Customer> customers = new List<Customer>();
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                customers = connection.Query<Customer>("Select * FROM Customer ORDER BY CreatedDate Desc").Where(q => q.UserName != "walletuser").ToList();

                connection.Close();
            }

            return customers;
        }

        public Customer UssdUserLogin(long customerId, string pin, out int loginAttempts)
        {
            try
            {
                loginAttempts = 0;
                Customer customer = GetCustomerByCustomerId(customerId);

                if (customer == null)
                {
                    throw new Exception(string.Format("CA0004 - CustomerID '{0}' does not exist", customerId));
                }

                if (customer.Nonce == null)
                {
                    throw new Exception("CA0001 - Customer PIN not set");
                }

                if (customer.TermsAccepted == false)
                {
                    throw new Exception("CA0005 - Customer has not accepted Terms And Conditions");
                }

                if (customer.BlacklistReasonId > 1)
                {
                    throw new Exception("CA0006 - Customer is blacklisted therefore cannot login.");
                }

                ManagePin managePin = new ManagePin();
                long passwordHashKeyId = long.Parse(EncDec.Decrypt(customer.Nonce, customer.Salt));

                bool validPin = managePin.ValidatePin(passwordHashKeyId, pin);

                if (validPin)
                {
                    customer.LoginAttempts = 0;
                }
                else
                {
                    if (customer.LoginAttempts < 2)
                    {
                        customer.LoginAttempts += 1;
                    }
                    else
                    {
                        customer.LoginAttempts += 1;
                        customer.IsBlacklisted = true;
                        customer.BlacklistReasonId = (int)BlacklistReasons.PinBlocked; /*Pin blocked for entering it incorrectly 3 times*/
                    }

                    loginAttempts = customer.LoginAttempts;

                    return null;
                }

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    var affectedRows = connection.Execute("UPDATE Customer SET LoginAttempts=@LoginAttempts, IsBlacklisted=@IsBlacklisted, BlacklistReasonId=@BlacklistReasonId WHERE CustomerId=@CustomerId", new { customer.LoginAttempts, customer.IsBlacklisted, customer.BlacklistReasonId, customer.CustomerId });

                    connection.Close();
                }

                return GetCustomerByCustomerId(customerId);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool AcceptTermsAndConditions(long customerId)
        {
            try
            {
                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    var affectedRows = connection.Execute("UPDATE Customer SET TermsAccepted=@TermsAccepted, IsBlacklisted=@IsBlacklisted, BlacklistReasonId=@BlacklistReasonId, TermsAcceptedDate=@TermsAcceptedDate WHERE CustomerId=@CustomerId", new { TermsAccepted = true, IsBlacklisted = false, BlacklistReasonId = (int)BlacklistReasons.Active, TermsAcceptedDate = GetRealDate(), CustomerId = customerId });

                    connection.Close();

                    if (affectedRows == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool SetCustomerPin(long customerId, string pin)
        {
            try
            {
                int pinValue = 0;

                bool pinParsedSuccessfully = int.TryParse(pin, out pinValue);

                if (!pinParsedSuccessfully)
                {
                    throw new Exception("CA0009 - Customer Pin is in invalid format");
                }

                Customer customer = GetCustomerByCustomerId(customerId);

                if (customer == null)
                {
                    throw new Exception(string.Format("CA0004 - CustomerID '{0}' does not exist", customerId));
                }

                if (!string.IsNullOrEmpty(customer.Nonce))
                {
                    throw new Exception("CA0002 - Customer PIN already set. Only Change PIN or Reset PIN can be used");
                }

                int customerPinLength = 4;

                if (pin.Length != customerPinLength)
                {
                    throw new Exception(string.Format("CA0003 - Invalid Customer PIN Length. The allowed Pin length is {0} Characters long.", customerPinLength));
                }

                ManagePin managePin = new ManagePin();
                long passwordHashKeyId = managePin.EncryptPin(pin, true);

                string salt = customer.Salt;

                if (string.IsNullOrEmpty(salt))
                {
                    salt = randomStringGenerator.NextString(256, true, true, true, true);
                    customer.Salt = salt;
                }

                customer.Nonce = EncDec.Encrypt(passwordHashKeyId.ToString(), salt);

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    var affectedRows = connection.Execute("UPDATE Customer SET Nonce=@Nonce, Salt=@Salt, BlacklistReasonId=@BlacklistReasonId, TermsAcceptedDate=@TermsAcceptedDate WHERE CustomerId=@CustomerId", new { customer.Nonce, salt, BlacklistReasonId = (int)BlacklistReasons.Active, TermsAcceptedDate = GetRealDate(), CustomerId = customerId });

                    connection.Close();

                    if (affectedRows == 1 && AcceptTermsAndConditions(customerId) == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public Customer GetOperatorTransactionsUser(out PaymentInstrument paymentInstrument)
        {
            paymentInstrument = null;
            Customer customer = GetCustomerByUsername("system");

            if (customer != null)
            {
                paymentInstrument = transactionRepository.GetPaymentInstrumentByCustomerId(customer.CustomerId);
            }
            return customer;
        }


        public Customer CustomerLogin(string msisdn, string Pin, out int loginAttempts)
        {
            try
            {
                loginAttempts = 0;
                Customer customer = GetCustomerByMsisdn(long.Parse(msisdn));

                if (customer == null)
                {
                    throw new Exception(string.Format("CA0004 - Phone No '{0}' does not exist", msisdn));
                }

                if (customer.Nonce == null)
                {
                    throw new Exception("CA0001 - Customer PIN not set");
                }

                if (customer.TermsAccepted == false)
                {
                    throw new Exception("CA0005 - Customer has not accepted Terms And Conditions");
                }

                if (customer.BlacklistReasonId > 1)
                {
                    throw new Exception("CA0006 - Customer is blacklisted therefore cannot login.");
                }

                ManagePin managePin = new ManagePin();
                long passwordHashKeyId = long.Parse(EncDec.Decrypt(customer.Nonce, customer.Salt));

                bool validPin = managePin.ValidatePin(passwordHashKeyId, Pin);

                if (validPin)
                {
                    customer.LoginAttempts = 0;
                }
                else
                {
                    if (customer.LoginAttempts < 2)
                    {
                        customer.LoginAttempts += 1;
                    }
                    else
                    {
                        customer.LoginAttempts += 1;
                        customer.IsBlacklisted = true;
                        customer.BlacklistReasonId = (int)BlacklistReasons.PinBlocked; /*Pin blocked for entering it incorrectly 3 times*/
                    }

                    using (var connection = new SqlConnection(sqlConnectionString))
                    {
                        connection.Open();
                        var affectedRows = connection.Execute("UPDATE Customer SET LoginAttempts=@LoginAttempts, IsBlacklisted=@IsBlacklisted, BlacklistReasonId=@BlacklistReasonId WHERE CustomerId=@CustomerId", new { LoginAttempts = customer.LoginAttempts, IsBlacklisted = customer.IsBlacklisted, BlacklistReasonId = customer.BlacklistReasonId, CustomerId = customer.CustomerId });

                        connection.Close();
                    }
                    loginAttempts = customer.LoginAttempts;
                    return null;
                }

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    var affectedRows = connection.Execute("UPDATE Customer SET LoginAttempts=@LoginAttempts, IsBlacklisted=@IsBlacklisted, BlacklistReasonId=@BlacklistReasonId WHERE CustomerId=@CustomerId", new { LoginAttempts = customer.LoginAttempts, IsBlacklisted = customer.IsBlacklisted, BlacklistReasonId = customer.BlacklistReasonId, CustomerId = customer.CustomerId });

                    connection.Close();
                }

                return customer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool ChangeCustomerPIN(string msisdn, string oldPin, string newPin)
        {
            DateTime transactionTime = GetRealDate();
            //bool changePinResult = false;

            Customer customer = null;

            int loginAttempts = 0;
            customer = CustomerLogin(msisdn, oldPin, out loginAttempts);

            if (customer != null)
            {
                try
                {
                    if (customer.Nonce == null)
                    {
                        throw new Exception("CA0001 - Customer PIN not set");
                    }

                    if (!customer.TermsAccepted)
                    {
                        throw new Exception("CA0005 - Customer has not accepted Terms And Conditions");
                    }

                    if (customer.BlacklistReasonId > (int)BlacklistReasons.Active)
                    {
                        throw new Exception("CA0006 - Customer is blacklisted therefore cannot login.");
                    }

                    long currentPasswordHashKeyId = long.Parse(EncDec.Decrypt(customer.Nonce, customer.Salt));

                    PasswordHashKey currentPhk = null;
                    //Fetch the password harshkey record being accessed
                    using (var connection = new SqlConnection(sqlConnectionString))
                    {
                        currentPhk = connection.Query<PasswordHashKey>("SELECT * FROM PasswordHashKey WHERE PasswordHashKeyId=@PasswordHashKeyId", new { PasswordHashKeyId = currentPasswordHashKeyId }).SingleOrDefault();
                    }
                    //Delete the record
                    using (var connection = new SqlConnection(sqlConnectionString))
                    {
                        var affectedRows = connection.Execute("DELETE PasswordHashKey WHERE PasswordHashKeyId=@PasswordHashKeyId", new { PasswordHashKeyId = currentPhk.PasswordHashKeyId });
                    }

                    customer.Nonce = null;
                    int pinValue = 0;
                    bool pinParsedSuccessfully = int.TryParse(newPin, out pinValue);

                    if (!pinParsedSuccessfully)
                    {
                        throw new Exception("CA0009 - Customer Pin is in invalid format");
                    }

                    customer = GetCustomerByMsisdn(long.Parse(msisdn));

                    int customerPinLength = 4;

                    if (newPin.Length != customerPinLength)
                    {
                        throw new Exception(string.Format("CA0003 - Invalid Customer PIN Length. The allowed Pin length is {0} Characters long.", customerPinLength));
                    }

                    ManagePin managePin = new ManagePin();
                    long passwordHashKeyId = managePin.EncryptPin(newPin, true);

                    string salt = customer.Salt;

                    if (string.IsNullOrEmpty(salt))
                    {
                        salt = randomStringGenerator.NextString(256, true, true, true, true);
                        customer.Salt = salt;
                    }

                    customer.Nonce = EncDec.Encrypt(passwordHashKeyId.ToString(), salt);

                    //Update the new PIN
                    using (var connection = new SqlConnection(sqlConnectionString))
                    {
                        connection.Open();
                        var affectedRows = connection.Execute("UPDATE Customer SET Nonce=@Nonce, Salt=@Salt, BlacklistReasonId=@BlacklistReasonId WHERE CustomerId=@CustomerId", new { Nonce = customer.Nonce, Salt = customer.Salt, CustomerId = customer.CustomerId, BlacklistReasonId = customer.BlacklistReasonId });

                        connection.Close();
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                //changePinResult = false;
                throw new Exception("Invalid customer login");
            }
        }



        public DateTime GetRealDate()
        {
            //Set the time zone information to E. Africa Standard Time 
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time");
            //Get date and time in US Mountain Standard Time 
            DateTime dateTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);

            return dateTime;
        }

        public Customer GetCustomerByReferalNo(string referalNo)
        {
            Customer customer = null;

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                customer = connection.Query<Customer>("SELECT * FROM Customer WHERE RefNo=@RefNo", new { RefNo = referalNo }).SingleOrDefault();
            }

            if (customer != null) { return customer; } else { return null; }
        }

        //public bool BlacklistAccount(long customerId, int blacklistReasonId, string blacklistedByUsername, string blacklistDescription)
        //{
        //    try
        //    {
        //        Customer customer = GetCustomerByCustomerId(customerId);
        //        Customer blackListedUser = GetCustomerByUsername(blacklistedByUsername);

        //        if (customer == null)
        //        {
        //            throw new Exception(string.Format("CA0004 - CustomerID '{0}' does not exist", customerId));
        //        }

        //        if (blackListedUser == null)
        //        {
        //            throw new Exception(string.Format("CA0011 - The Username '{0}' does not exist", blacklistedByUsername));
        //        }

        //        if (string.IsNullOrWhiteSpace(blacklistDescription) && blacklistReasonId != (int)BlacklistReasons.Active && blacklistReasonId != (int)BlacklistReasons.PinBlocked)
        //        {
        //            throw new Exception("CA0031 - The reason for Blacklisting an account must be provided");
        //        }

        //        customer.IsBlacklisted = true;

        //        if (blacklistReasonId == (int)BlacklistReasons.Active/*Active Status blacklist*/)
        //        {
        //            customer.IsBlacklisted = false;
        //            //pick the last record and update the fields
        //            BlacklistRecord record = context.BlacklistRecords.Where(last => last.CustomerId == customerId && last.BlacklistReasonId != (int)BlacklistReasons.Active).OrderByDescending(x => x.BlacklistRecordId).FirstOrDefault();
        //            if (record != null)
        //            {
        //                record.WhitelistedByUsername = blacklistedByUsername;
        //                record.WhitelistedDate = dateTime;
        //                record.IsWhitelisted = true;
        //            }

        //            if (!customer.TermsAccepted)
        //            {
        //                customer.TermsAccepted = true;
        //                customer.TermsAcceptedDate = dateTime;
        //            }
        //        }

        //        // Customer status is changing from a PI Blocked status to a whitelisted one
        //        if (customer.BlacklistReasonId == (int)BlacklistReasons.PinBlocked && blacklistReasonId == (int)BlacklistReasons.Active)
        //        {
        //            customer.LoginAttempts = 0;
        //        }

        //        customer.BlacklistReasonId = blacklistReasonId;

        //        if (blacklistReasonId != (int)BlacklistReasons.Active)
        //        {
        //            BlacklistRecord blacklistRecord = new BlacklistRecord
        //            {
        //                CustomerId = customerId,
        //                BlacklistReasonId = blacklistReasonId,
        //                BlacklistedByUsername = blacklistedByUsername,
        //                BlacklistDescription = blacklistDescription,
        //                BlacklistedDate = dateTime,
        //                IsWhitelisted = false
        //            };

        //            context.BlacklistRecords.Add(blacklistRecord);
        //        }
        //        context.SaveChanges();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

    }
}