using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

using Dapper;

using Hook.Enums;
using Hook.Helper;
using Hook.Models;

namespace Hook.Repository
{
    public class BillPaymentRepository
    {

         string sqlConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString();

        public Biller CreateBiller(int supportApplicationId, long customerId, long notificationCustomerId, int billerTypeId, string billerName, string billerNumber, string emailAddress, string businessLocation, long subsidizedAmount, DateTime terminationDate, long operationPaymentInstrumentId, long overflowPaymentInstrumentId, bool isActive, bool showInUI)
        {
            CustomerRepository customerRepository = new CustomerRepository();
            TransactionRepository transactionRepository = new TransactionRepository();
            try
            {
                DateTime transactiontime = GetRealDate();
                //TODO Validate email address

                //TODO Check if the phone number is already being used by another customer other than the biller

                Customer notificationCustomer = null;

                if (notificationCustomerId > 0)
                {
                    notificationCustomer = customerRepository.GetCustomerByCustomerId(notificationCustomerId);

                    if (notificationCustomer == null)
                    {
                        throw new Exception(string.Format("CA0004 - CustomerId {0} does not exist", notificationCustomer), new Exception("Invalid CustomerId"));
                    }
                }

                Customer existingCustomer = customerRepository.GetCustomerByCustomerId(customerId);

                if (existingCustomer == null)
                {
                    throw new Exception(string.Format("CA0004 - CustomerId {0} does not exist", customerId), new Exception("Invalid CustomerId"));
                }

                if (existingCustomer.CustomerTypeId != 3 || existingCustomer.CustomerTypeId != 2 || existingCustomer.CustomerTypeId != 5) //Unacceptable Customer Type
                {
                    throw new Exception("BL0006 - Unacceptable Customer type", new Exception("Only a customer of Type Biller can be created as a Biller"));
                }

                if (existingCustomer.UserTypeId == 1)
                {
                    throw new Exception("BL0005 - Unacceptable User type", new Exception("Only a customer of User Type Company can be created as a Biller"));
                }

                if (string.IsNullOrEmpty(businessLocation))
                {
                    throw new Exception("BL0020	- Biller’s BusinessLocation is not set.");
                }

                //if (context.Billers.Any(x => x.BillerName == billerName))
                //{
                //    throw new Exception(String.Format("BL0004 - Biller Name '{0}' already Exists.", billerName));
                //}


                if (GetBillerByBillerNumber(billerNumber).BillerId > 0)
                {
                    throw new Exception("BL0007	- Biller Number already assigned to another account", new Exception("The biller number has already been assigned to another biller. Choose a different one"));
                }


                if (operationPaymentInstrumentId != 0)
                {
                    if (transactionRepository.GetPaymentInstrumentByPaymentInstrumentId(operationPaymentInstrumentId) == null)
                    {
                        throw new Exception(string.Format("PI0001 - Payment Instrument '{0}' doesn’t exist", operationPaymentInstrumentId));
                    }
                }

                if (overflowPaymentInstrumentId != 0)
                {
                    if (transactionRepository.GetPaymentInstrumentByPaymentInstrumentId(overflowPaymentInstrumentId) == null)
                    {
                        throw new Exception(string.Format("PI0001 - Payment Instrument '{0}' doesn’t exist", overflowPaymentInstrumentId));
                    }
                }

                existingCustomer.CustomerTypeId = (int)CustomerTypes.Biller;

                PaymentInstrument billerPaymentInstrument = transactionRepository.CreatePaymentInstrument(customerId);

                Biller newBiller = new Biller
                {
                    BillerName = billerName,
                    CustomerId = customerId,
                    BillerNumber = billerNumber,
                    SubsidizedAmount = subsidizedAmount,
                    TerminationDate = terminationDate.ToShortDateString() == GetRealDate().ToShortDateString() ? GetRealDate().AddYears(100) : terminationDate,
                    SupportApplicationId = supportApplicationId,
                    BlackListReasonId = 1,
                    NotificationCustomerId = notificationCustomerId == 0 ? customerId : notificationCustomerId,
                    BillerEmailAddress = string.IsNullOrEmpty(emailAddress) ? null : emailAddress,
                    BusinessLocation = businessLocation,
                    IsActive = isActive,
                    CreatedDate = transactiontime,
                    BillerTypeId = billerTypeId,
                    OperationPaymentInstrumentId = operationPaymentInstrumentId == 0 ? billerPaymentInstrument.PaymentInstrumentId : operationPaymentInstrumentId,
                    OverflowPaymentInstrumentId = overflowPaymentInstrumentId,
                    IpnActive = false,
                    IpnUrl = string.Empty,
                    ServiceOnline = true,
                    NotifyCustomersOnResume = true,
                    ShowInUI = showInUI
                };

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();

                    var affectedRows = connection.Execute("INSERT INTO Biller (BillerName,CustomerId,BillerNumber,SubsidizedAmount,TerminationDate,SupportApplicationId,BlackListReasonId,NotificationCustomerId,BillerEmailAddress,BusinessLocation,IsActive,CreatedDate,BillerTypeId,OperationPaymentInstrumentId,OverflowPaymentInstrumentId,IpnActive,IpnUrl) VALUES (@BillerName,@CustomerId,@BillerNumber,@SubsidizedAmount,@TerminationDate,@SupportApplicationId,@BlackListReasonId,@NotificationCustomerId,@BillerEmailAddress,@BusinessLocation,@IsActive,@CreatedDate,@BillerTypeId,@OperationPaymentInstrumentId,@OverflowPaymentInstrumentId,@IpnActive,@IpnUrl)", new { newBiller.BillerName, newBiller.CustomerId, newBiller.BillerNumber, newBiller.SubsidizedAmount, newBiller.TerminationDate, newBiller.SupportApplicationId, newBiller.BlackListReasonId, newBiller.NotificationCustomerId, newBiller.BillerEmailAddress, newBiller.BusinessLocation, newBiller.IsActive, newBiller.CreatedDate, newBiller.BillerTypeId, newBiller.OperationPaymentInstrumentId, newBiller.OverflowPaymentInstrumentId, newBiller.IpnActive, newBiller.IpnUrl });

                    connection.Close();
                }

                //  messageRepository.BillerCreatedSms(newBiller, transactiontime);

                return GetBillerByBillerNumber(newBiller.BillerNumber);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Biller GetBillerByBillerNumber(string billerNumber)
        {
            try
            {
                Biller biller = null;

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();

                    biller = connection.Query<Biller>("SELECT * FROM Biller WHERE BillerNumber=@BillerNumber", new { BillerNumber = billerNumber }).SingleOrDefault();
                    connection.Close();
                }

                if (biller == null) return biller;

                return EnforceBillerValidity(biller.BillerId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Biller GetBillerByCustomerId(long customerId)
        {
            Biller biller = null;

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                biller = connection.Query<Biller>("SELECT * FROM Biller WHERE CustomerId=@CustomerId AND IsActive=@IsActive", new { CustomerId = customerId, IsActive = true }).SingleOrDefault();
                connection.Close();
            }

            if (biller == null) return biller;

            return EnforceBillerValidity(biller.BillerId);
        }

        private Biller EnforceBillerValidity(int billerId)
        {
            Biller biller = GetBillerByBillerId(billerId);

            if (biller.TerminationDate < GetRealDate())
            {
                List<Biller> billers = null;

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();

                    billers = connection.Query<Biller>("SELECT * FROM Biller WHERE CustomerId=@CustomerId", new { CustomerId = biller.CustomerId }).ToList();
                    connection.Close();
                }

                if (billers.Count == 1)
                {
                    using (var connection = new SqlConnection(sqlConnectionString))
                    {
                        connection.Open();
                        var affectedRows = connection.Execute("UPDATE Biller SET ServiceOnline=@ServiceOnline, IsActive=@IsActive WHERE BillerId = @BillerId", new { ServiceOnline = false, IsActive = false, BillerId = billerId });

                        connection.Close();
                    }

                }
            }

            return biller;
        }

        public Biller GetBillerByBillerId(int billerId)
        {
            Biller biller = null;

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                biller = connection.Query<Biller>("SELECT * FROM Biller WHERE BillerId=@BillerId", new { BillerId = billerId }).SingleOrDefault();
                connection.Close();
            }
            return biller;
        }

        public DateTime GetRealDate()
        {
            //Set the time zone information to E. Africa Standard Time 
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time");
            //Get date and time in US Mountain Standard Time 
            DateTime dateTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);

            return dateTime;
        }

        //public BillerSetting CreateBillerSetting(string billerSettingName)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(billerSettingName))
        //        {
        //            throw new Exception("BL0015 - Biller SettingName must be set.");
        //        }

        //        if (context.BillerSettings.Count(setting => setting.SettingName.ToLower() == billerSettingName.ToLower()) > 0)
        //        {
        //            throw new Exception(String.Format("BL0020 - BillerSetting with SettingName '{0}' already exists.", billerSettingName));
        //        }
        //        BillerSetting newBillerSetting = new BillerSetting { SettingName = billerSettingName };

        //        context.BillerSettings.Add(newBillerSetting);
        //        context.SaveChanges();
        //        return newBillerSetting;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public BillerSettingConfiguration CreateBillerSettingConfiguration(int billerId, int billerSettingId, int? httpMethodId, string settingValue, string username, string password, string createdByUsername)
        //{
        //    try
        //    {
        //        HelperRepository helperRepository = new HelperRepository();

        //        if (string.IsNullOrEmpty(createdByUsername))
        //        {
        //            throw new Exception("CA0028 - Username must be set for this Action.");
        //        }

        //        if (string.IsNullOrEmpty(settingValue))
        //        {
        //            throw new Exception("BL0013 - Biller SettingValue must be set.");
        //        }

        //        helperRepository.ValidateUsername(createdByUsername, false);
        //        Biller biller = context.Billers.Find(billerId);
        //        BillerSetting billerSetting = context.BillerSettings.Find(billerSettingId);

        //        if (biller == null)
        //        {
        //            throw new Exception(String.Format("BL0008	- The BillerId '{0}' does not exist.", billerId));
        //        }

        //        DateTime dateCreated = DateTime.Now;

        //        if (biller.TerminationDate <= dateCreated)
        //        {
        //            throw new Exception("BL0010	- The Biller is not active. The termination date is passed.");
        //        }

        //        if (billerSetting == null)
        //        {
        //            throw new Exception(String.Format("BL001 - The BillerSettingId '{0}' does not exist.", billerSettingId));
        //        }

        //        ILoyal.DataLayer.Customer customer = context.Customers.Find(biller.CustomerId);

        //        if (customer == null)
        //        {
        //            throw new Exception(String.Format("CA0004 - CustomerId '{0}' does not exist.", biller.CustomerId));
        //        }

        //        string configurationKey = string.Empty;
        //        string configurationSecret = string.Empty;

        //        if (!string.IsNullOrEmpty(username))
        //        {
        //            configurationKey = EncDec.Encrypt(username, customer.Salt);
        //        }
        //        if (!string.IsNullOrEmpty(password))
        //        {
        //            configurationSecret = EncDec.Encrypt(password, customer.Salt);
        //        }

        //        BillerSettingConfiguration settingConfiguration = new BillerSettingConfiguration
        //        {
        //            BillerId = biller.BillerId,
        //            BillerSettingId = billerSetting.BillerSettingId,
        //            SettingValue = settingValue,
        //            ConfigurationKey = string.IsNullOrEmpty(configurationKey) ? null : configurationKey,
        //            ConfigurationSecret = string.IsNullOrEmpty(configurationSecret) ? null : configurationSecret,
        //            IsActive = true,
        //            CreatedByUsername = createdByUsername.ToLower(),
        //            CreatedDate = dateCreated,
        //            HttpMethodId = httpMethodId ?? null
        //        };

        //        context.BillerSettingConfigurations.Add(settingConfiguration);
        //        context.SaveChanges();

        //        return settingConfiguration;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public Biller CreateBusinessAdministrationAccount()
        //{
        //    return CreateSystemBiller(applicationSettings.InstanceName + " Business Administration Biller", "000002", 254999000002);
        //}

        //public Biller CreateCommissionPaymentsBiller()
        //{
        //    Biller commissionsBiller = CreateSystemBiller(applicationSettings.InstanceName + " CommissionPayments Biller", "000004", 254999000004);

        //    PaymentInstrument commissionsPi = context.PaymentInstruments.Find(commissionsBiller.OperationPaymentInstrumentId);
        //    commissionsPi.PaymentInstrumentTypeId = (int)PITypes.MPESA;
        //    commissionsPi.PaymentIntrumentAlias = "Commission SVA";
        //    commissionsPi.AccountNumber = "CommissionSva";
        //    context.SaveChanges();
        //    return commissionsBiller;
        //}

        //public Biller CreateThirdPartyDisbursementsBiller()
        //{
        //    return CreateSystemBiller(applicationSettings.InstanceName + " Third-PartyDisbursement Biller", "000003", 254999000003);
        //}


        //public bool FlipBillerUserInterfaceVisibility(int billerId, bool showInUI)
        //{
        //    try
        //    {
        //        Biller billerToFlip = context.Billers.Find(billerId);
        //        if (billerToFlip == null)
        //        {
        //            throw new Exception(String.Format("BL0008 - The BillerId '{0}' does not exist.", billerId));
        //        }

        //        billerToFlip.ShowInUI = showInUI;
        //        context.SaveChanges();

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public MasterTransactionRecord GenericBillerTransaction(TransactionTypes transactionTypes, long sendingCustomerId, long sendingPaymentInstrumentId, long recievingCustomerId,
        //            long recievingPaymentInstrumentId, long amount, string transactionText, int billerId, string thirdPartyTransactionId = "")
        //{
        //    //WalletTransactionsRepository walletTransactionsRepository = new WalletTransactionsRepository(this.);
        //    HelperRepository helperRepository = new HelperRepository();

        //    TransactionType transactionType = helperRepository.GetTransactionTypeByTransactionTypeId((int)transactionTypes);
        //    if (!transactionType.IsActive)
        //        return null;

        //    if (!string.IsNullOrEmpty(thirdPartyTransactionId))
        //    {
        //        MasterTransactionRecord thirdPartyTransactionRecord = helperRepository.VerifyThirdPartyTransactionExistence(thirdPartyTransactionId);
        //        if (thirdPartyTransactionRecord != null)
        //        {
        //            return thirdPartyTransactionRecord;
        //        }
        //    }


        //    DateTime transactionTime = DateTime.Now;

        //    Customer sendingCustomer = null;
        //    PaymentInstrument sendingPaymentInstrument = null;

        //    walletTransactionsRepository.MatchPaymentInstrumentToCustomer(sendingCustomerId, sendingPaymentInstrumentId, out sendingCustomer, out sendingPaymentInstrument);

        //    TransactionLimitType transactionLimitType = helperRepository.TransactionLimitTypeExists((int)TransactionLimitsType.DailyLimit);
        //    helperRepository.ValidateCumulativeCustomerTransactionLimit(sendingCustomerId, sendingPaymentInstrument, amount, transactionLimitType);

        //    Customer recievingCustomer = null;
        //    PaymentInstrument recievingPaymentInstrument = null;

        //    walletTransactionsRepository.MatchPaymentInstrumentToCustomer(recievingCustomerId, recievingPaymentInstrumentId, out recievingCustomer, out recievingPaymentInstrument);

        //    //if (sendingCustomer.CustomerTypeId != (int)CustomerTypes.Agent)
        //    //{
        //    //    throw new Exception("AG0004 - The specified tillCustomerId '" + sendingbiller.CustomerId.ToString() + "' does not belong to any Agent");
        //    //}

        //    //if (recievingBiller.CustomerTypeId != (int)CustomerTypes.sendingbiller)
        //    //{
        //    //    throw new Exception("BL0011 - The specified CustomerId '" + sendingbiller.CustomerId.ToString() + "' does not belong to any sendingbiller");
        //    //}


        //    //if (!AgentHasFundsForTransaction(outlet.TillCustomerId, amount))
        //    //{
        //    //    throw new Exception("AG0003 - Agent has insufficient funds to conduct this transaction");
        //    //}

        //    //bool defaultTransactionFee = false;
        //    long feeAmount = 0;
        //    string transactionReference = HelperRepository.GenerateTransactionReferenceNumber();

        //    decimal senderBalanceBeforeTransaction = walletTransactionsRepository.BalanceEnquiry(sendingCustomerId, sendingPaymentInstrumentId, string.Empty, false);
        //    decimal receiverBalanceBeforeTransaction = walletTransactionsRepository.BalanceEnquiry(recievingCustomerId, recievingPaymentInstrumentId, string.Empty, false);

        //    HelperRepository.CustomerHasSufficientFundsForTransaction(senderBalanceBeforeTransaction, amount, feeAmount);

        //    MasterTransactionRecord newAgentSendMoneyTransaction = new MasterTransactionRecord
        //    {
        //        TransactionTypeId = transactionType.TransactionTypeId,
        //        PayerId = sendingCustomerId,
        //        PayeeId = recievingCustomerId,
        //        PayerPaymentInstrumentId = sendingPaymentInstrumentId,
        //        PayeePaymentInstrumentId = recievingPaymentInstrumentId,
        //        ExternalApplicationId = 1,
        //        AccessChannelId = (int)AccessChannels.USSD,
        //        Amount = amount,
        //        Fee = feeAmount,
        //        CustomerTypeId = sendingCustomer.CustomerTypeId,
        //        Tax = 0,
        //        Text = transactionText,
        //        TransactioErrorCodeId = 1,
        //        IsTestTransaction = sendingCustomer.IsTestCustomer,
        //        TransactionDate = transactionTime,
        //        TransactionReference = transactionReference,
        //        SourceUserName = sendingCustomer.UserName,
        //        DestinationUserName = recievingCustomer.UserName,
        //        ShortDescription = transactionType.FriendlyName,
        //        PayerBalanceBeforeTransaction = long.Parse(senderBalanceBeforeTransaction.ToString()),
        //        PayeeBalanceBeforeTransaction = long.Parse(receiverBalanceBeforeTransaction.ToString()),
        //        TransactionStatusId = TransactionState.Successful
        //    };


        //    long agentCommssion = 0;

        //    CustomerTransactionGL customerTransactionGL = new CustomerTransactionGL
        //    {
        //        CustomerId = recievingCustomerId,
        //        MasterTransactionRecordId = newAgentSendMoneyTransaction.MasterTransactionRecordId,
        //        TransactionDate = transactionTime
        //    };

        //    //==========================================================================
        //    //TODO - this assumption is very incorrect
        //    Biller sendingbiller = context.Billers.Find(billerId);
        //    //==========================================================================

        //    BillerTransactionGL billerTransactionGL = new BillerTransactionGL
        //    {
        //        Amount = amount,
        //        BillerId = sendingbiller.BillerId,
        //        CommssionEarned = 0,
        //        MasterTransactionRecordId = newAgentSendMoneyTransaction.MasterTransactionRecordId,
        //        PaymentInstrumentId = sendingbiller.OperationPaymentInstrumentId,
        //        TransactionDate = transactionTime
        //    };


        //    //FeesGL feeGl = new FeesGL
        //    //{
        //    //    MasterTransactionRecordId = newAgentSendMoneyTransaction.MasterTransactionRecordId,
        //    //    CustomerFeeAmount = feeAmount,
        //    //    TransactionCommission = agentCommssion,
        //    //    GrossRevenue = feeAmount - agentCommssion,
        //    //    TransactionDate = transactionTime,
        //    //    TransactionDescription = transactionType.FriendlyName
        //    //};

        //    HelperRepository.UpdateSVABalance(context, sendingbiller.CustomerId, sendingbiller.OperationPaymentInstrumentId, amount, transactionReference, false, transactionText);
        //    HelperRepository.UpdateCustomerSVABalance(context, recievingCustomerId, recievingPaymentInstrument, amount, feeAmount, true);

        //    context.MasterTransactionRecords.Add(newAgentSendMoneyTransaction);
        //    context.CustomerTransactionGLs.Add(customerTransactionGL);
        //    context.BillerTransactionGLs.Add(billerTransactionGL);
        //    // context.AgentTansactionGLs.Add(agentTransactionGL);
        //    //context.FeesGLs.Add(feeGl);
        //    context.SaveChanges();

        //    walletTransactionsRepository = new WalletTransactionsRepository();

        //    decimal senderBalanceAfterTransaction = walletTransactionsRepository.BalanceEnquiry(sendingCustomerId, sendingPaymentInstrumentId, string.Empty, false);
        //    decimal receiverBalanceAfterTransaction = walletTransactionsRepository.BalanceEnquiry(recievingCustomerId, recievingPaymentInstrumentId, string.Empty, false);

        //    newAgentSendMoneyTransaction.PayerBalanceAfterTransaction = long.Parse(senderBalanceAfterTransaction.ToString());
        //    newAgentSendMoneyTransaction.PayeeBalanceAfterTransaction = long.Parse(receiverBalanceAfterTransaction.ToString());

        //    context.SaveChanges();

        //    helperRepository.UpdateCumulativeCustomerTransactionLimit(sendingCustomerId, sendingPaymentInstrument, amount);

        //    return newAgentSendMoneyTransaction;
        //}

        //public MasterTransactionRecord BillerDisbursement(TransactionTypes transactionTypes, long sendingCustomerId, long sendingPaymentInstrumentId, long recievingCustomerId, long recievingPaymentInstrumentId, long amount, string transactionText, int billerId)
        //{
        //    Biller sendingbiller = BillerIdExists(billerId);

        //    HelperRepository helperRepository = new HelperRepository();
        //    TransactionType transactionType = helperRepository.GetTransactionTypeByTransactionTypeId((int)transactionTypes);

        //    if (!transactionType.IsActive) return new MasterTransactionRecord { TransactionStatusId = TransactionState.Failed };

        //    DateTime transactionTime = DateTime.Now;

        //    ILoyal.DataLayer.Customer sendingCustomer = null;
        //    PaymentInstrument sendingPaymentInstrument = null;

        //    walletTransactionsRepository.MatchPaymentInstrumentToCustomer(sendingCustomerId, sendingPaymentInstrumentId, out sendingCustomer, out sendingPaymentInstrument);

        //    TransactionLimitType transactionLimitType = helperRepository.TransactionLimitTypeExists((int)TransactionLimitsType.DailyLimit);
        //    helperRepository.ValidateCumulativeCustomerTransactionLimit(sendingCustomerId, sendingPaymentInstrument, amount, transactionLimitType);

        //    ILoyal.DataLayer.Customer recievingCustomer = null;
        //    PaymentInstrument recievingPaymentInstrument = null;

        //    walletTransactionsRepository.MatchPaymentInstrumentToCustomer(recievingCustomerId, recievingPaymentInstrumentId, out recievingCustomer, out recievingPaymentInstrument);

        //    //if (sendingCustomer.CustomerTypeId != (int)CustomerTypes.Agent)
        //    //{
        //    //    throw new Exception("AG0004 - The specified tillCustomerId '" + sendingbiller.CustomerId.ToString() + "' does not belong to any Agent");
        //    //}

        //    //if (recievingBiller.CustomerTypeId != (int)CustomerTypes.sendingbiller)
        //    //{
        //    //    throw new Exception("BL0011 - The specified CustomerId '" + sendingbiller.CustomerId.ToString() + "' does not belong to any sendingbiller");
        //    //}


        //    //if (!AgentHasFundsForTransaction(outlet.TillCustomerId, amount))
        //    //{
        //    //    throw new Exception("AG0003 - Agent has insufficient funds to conduct this transaction");
        //    //}

        //    bool defaultTransactionFee = false;
        //    long tariffItemId = 0;
        //    long feeAmount = 0;
        //    //long feeAmount = tariffRepository.GetDisbursementTariffCharge(sendingbiller, (int)AccessChannels.Web, amount, out defaultTransactionFee, out tariffItemId);
        //    //bool defaultTransactionFee = false;
        //    //long feeAmount = 7500;

        //    string transactionReference = HelperRepository.GenerateTransactionReferenceNumber();

        //    decimal senderBalanceBeforeTransaction = walletTransactionsRepository.BalanceEnquiry(sendingCustomerId, sendingPaymentInstrumentId, string.Empty, false);
        //    decimal receiverBalanceBeforeTransaction = walletTransactionsRepository.BalanceEnquiry(recievingCustomerId, recievingPaymentInstrumentId, string.Empty, false);

        //    HelperRepository.CustomerHasSufficientFundsForTransaction(senderBalanceBeforeTransaction, amount, 0);
        //    walletTransactionsRepository.CheckConsumerUpperLimit(receiverBalanceBeforeTransaction, amount);

        //    MasterTransactionRecord disbursementTransaction = new MasterTransactionRecord
        //    {
        //        TransactionTypeId = transactionType.TransactionTypeId,
        //        PayerId = sendingCustomerId,
        //        PayeeId = recievingCustomerId,
        //        PayerPaymentInstrumentId = sendingPaymentInstrumentId,
        //        PayeePaymentInstrumentId = recievingPaymentInstrumentId,
        //        ExternalApplicationId = 1,
        //        AccessChannelId = (int)AccessChannels.Web,
        //        Amount = amount,
        //        Fee = feeAmount,
        //        CustomerTypeId = sendingCustomer.CustomerTypeId,
        //        Tax = 0,
        //        Text = transactionText,
        //        TransactioErrorCodeId = 1,
        //        IsTestTransaction = sendingCustomer.IsTestCustomer,
        //        TransactionDate = transactionTime,
        //        TransactionReference = transactionReference,
        //        SourceUserName = sendingCustomer.UserName,
        //        DestinationUserName = recievingCustomer.UserName,
        //        ShortDescription = transactionType.FriendlyName,
        //        PayerBalanceBeforeTransaction = long.Parse(senderBalanceBeforeTransaction.ToString()),
        //        PayeeBalanceBeforeTransaction = long.Parse(receiverBalanceBeforeTransaction.ToString()),
        //        TransactionStatusId = TransactionState.Successful
        //    };

        //    long agentCommssion = 0;

        //    CustomerTransactionGL customerTransactionGL = new CustomerTransactionGL
        //    {
        //        CustomerId = recievingCustomerId,
        //        MasterTransactionRecordId = disbursementTransaction.MasterTransactionRecordId,
        //        TransactionDate = transactionTime
        //    };

        //    //==========================================================================
        //    //TODO - this assumption is very incorrect
        //    //==========================================================================

        //    BillerTransactionGL billerTransactionGL = new BillerTransactionGL
        //    {
        //        Amount = amount,
        //        BillerId = sendingbiller.BillerId,
        //        CommssionEarned = 0,
        //        MasterTransactionRecordId = disbursementTransaction.MasterTransactionRecordId,
        //        PaymentInstrumentId = sendingbiller.OperationPaymentInstrumentId,
        //        TransactionDate = transactionTime
        //    };

        //    //FeesGL feeGl = new FeesGL
        //    //{
        //    //    MasterTransactionRecordId = disbursementTransaction.MasterTransactionRecordId,
        //    //    CustomerFeeAmount = feeAmount,
        //    //    TransactionCommission = agentCommssion,
        //    //    GrossRevenue = feeAmount - agentCommssion,
        //    //    TransactionDate = transactionTime,
        //    //    TransactionDescription = transactionType.FriendlyName
        //    //};

        //    HelperRepository.UpdateSVABalance(context, sendingbiller.OperationPaymentInstrumentId, amount, false);
        //    HelperRepository.UpdateSVABalance(context, recievingPaymentInstrument.PaymentInstrumentId, (amount - feeAmount), true);

        //    context.MasterTransactionRecords.Add(disbursementTransaction);
        //    context.CustomerTransactionGLs.Add(customerTransactionGL);
        //    context.BillerTransactionGLs.Add(billerTransactionGL);
        //    //context.FeesGLs.Add(feeGl);
        //    context.SaveChanges();

        //    walletTransactionsRepository = new WalletTransactionsRepository();
        //    decimal senderBalanceAfterTransaction = walletTransactionsRepository.BalanceEnquiry(sendingCustomerId, sendingPaymentInstrumentId, string.Empty, false);
        //    decimal receiverBalanceAfterTransaction = walletTransactionsRepository.BalanceEnquiry(recievingCustomerId, recievingPaymentInstrumentId, string.Empty, false);

        //    disbursementTransaction.PayerBalanceAfterTransaction = long.Parse(senderBalanceAfterTransaction.ToString());
        //    disbursementTransaction.PayeeBalanceAfterTransaction = long.Parse(receiverBalanceAfterTransaction.ToString());

        //    context.SaveChanges();

        //    helperRepository.UpdateCumulativeCustomerTransactionLimit(sendingCustomerId, sendingPaymentInstrument, amount);

        //    return disbursementTransaction;
        //}

        //public List<Biller> GetActiveUserInterfaceBillers()
        //{
        //    try
        //    {
        //        return context.Billers.Where(biller => biller.IsActive == true && biller.ShowInUI == true).ToList();
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public List<Biller> GetAllBillers()
        //{
        //    //using (ILoyalDataModel context = new ILoyalDataModel(_))
        //    //{
        //    try
        //    {
        //        return context.Billers.ToList();

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    // }
        //}

        //public Biller GetBillerByBillerId(int billerId)
        //{
        //    Biller biller = context.Billers.Find(billerId);
        //    if (biller == null) return biller;

        //    return EnforceBillerValidity(biller.BillerId);
        //}

        //public List<Biller> GetBillersByCustomerId(long customerId)
        //{
        //    List<Biller> billersToReturn = new List<Biller>();
        //    List<Biller> billers = context.Billers.Where(x => x.CustomerId == customerId && x.IsActive == true).ToList();
        //    if (billers.Count < 1) return billers;
        //    billers.ForEach(biller =>
        //    {
        //        billersToReturn.Add(EnforceBillerValidity(biller.BillerId));
        //    });

        //    return billersToReturn;
        //}

        //public BillerSetting GetBillerSettingBySettingId(int settingId)
        //{
        //    try
        //    {
        //        return context.BillerSettings.Find(settingId);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public List<BillerSettingConfiguration> GetBillerSettingConfigurations(int billerId, int settingTypeId)
        //{
        //    try
        //    {
        //        if (settingTypeId == 0)
        //        {
        //            return context.BillerSettingConfigurations.Where(configSetting => configSetting.BillerId == billerId && configSetting.IsActive == true).ToList();
        //        }
        //        else
        //        {
        //            return context.BillerSettingConfigurations.Where(configSetting => configSetting.BillerId == billerId && configSetting.BillerSettingId == settingTypeId && configSetting.IsActive == true).ToList();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public List<BillerSettingConfiguration> GetBillerSettingConfigurationsByCustomerId(long customerId, int settingTypeId = 0)
        //{
        //    try
        //    {
        //        if (settingTypeId == 0)
        //        {
        //            return context.BillerSettingConfigurations.Where(configSetting => configSetting.Biller.CustomerId == customerId && configSetting.IsActive == true).ToList();
        //        }
        //        else
        //        {
        //            return context.BillerSettingConfigurations.Where(configSetting => configSetting.Biller.CustomerId == customerId && configSetting.BillerSettingId == settingTypeId && configSetting.IsActive == true).ToList();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public void ReplaySalaryDisbursementSms()
        //{

        //    List<MasterTransactionRecord> smsToReplay = context.MasterTransactionRecords.Where(x => (x.PayerId == 6273 && x.PayerPaymentInstrumentId == 353479 && x.TransactionTypeId == 57)).ToList();

        //    foreach (MasterTransactionRecord record in smsToReplay)
        //    {
        //        if (record.PayeeId == 66232 && record.PayeePaymentInstrumentId == 67128)
        //        { }
        //        else
        //        {
        //            ILoyal.DataLayer.Customer customer = _customerRepository.GetCustomerByCustomerId(record.PayeeId);
        //            long recipientMsisdn = _customerRepository.GetCustomerPhoneNumber(record.PayeeId);

        //            string smsText = string.Format("{0} Confirmed. Dear {1}, You have received Ksh {2} from {3}. Your new DirectCore balance is Ksh {4}. In case of any query contact customer care on 0703066000.",
        //            record.TransactionReference, (String.Format("{0} {1}", customer.FirstName, customer.LastName)), "XXX", "DirectCore Direct", "YYY");

        //            Parallel.Invoke(() =>
        //            {
        //                // messageRepository.SendDisbursementSms1(record, record.Amount, record.PayeeBalanceAfterTransaction.Value, smsText, recipientMsisdn);
        //            },
        //            () =>
        //            {
        //                // analytics
        //            });
        //        }
        //    }
        //}

        //public Biller UpdateBiller(int supportApplicationId, long billerId, long notificationCustomerId, int billerTypeId, string billerName, string billerNumber, string emailAddress, string businessLocation, long subsidizedAmount, DateTime terminationDate, long operationPaymentInstrumentId, long overflowPaymentInstrumentId, bool isActive, bool showInUI)
        //{
        //    try
        //    {
        //        //TODO Validate email address

        //        //TODO Check if the phone number is already being used by another customer other than the biller

        //        Customer notificationCustomer = null;

        //        if (notificationCustomerId > 0)
        //        {
        //            notificationCustomer = _customerRepository.GetCustomerByCustomerId(notificationCustomerId);

        //            if (notificationCustomer == null)
        //            {
        //                throw new Exception(String.Format("CA0004 - CustomerId {0} does not exist", notificationCustomer), new Exception("Invalid CustomerId"));
        //            }
        //        }
        //        Biller biller = context.Billers.Find(billerId);

        //        if (biller == null)
        //        {
        //            throw new Exception(String.Format(" BL0008 - The BillerId '{0}' does not exist.", billerId));
        //        }

        //        Customer existingCustomer = _customerRepository.GetCustomerByCustomerId(biller.CustomerId);

        //        if (existingCustomer == null)
        //        {
        //            throw new Exception("CA0004 - CustomerId " + biller.CustomerId.ToString() + " does not exist", new Exception("Invalid CustomerId"));
        //        }

        //        if (existingCustomer.CustomerTypeId != 3) //Unacceptable Customer Type
        //        {
        //            throw new Exception("BL0006 - Unacceptable Customer type", new Exception("Only a customer of Type Biller can be created as a Biller"));
        //        }

        //        //if (existingCustomer.UserTypeId == 1)
        //        //{
        //        //    throw new Exception("BL0005 - Unacceptable User type", new Exception("Only a customer of User Type Company can be created as a Biller"));
        //        //}


        //        //if (context.Billers.Count(x => x.BillerName == billerName) > 0 && billerName != biller.BillerName)
        //        //{
        //        //    throw new Exception("BL0004 - Biller Name'" + billerName + "' already Exists" );
        //        //}


        //        if (context.Billers.Count(x => x.BillerNumber == billerNumber) > 0 && billerNumber != biller.BillerNumber)
        //        {
        //            throw new Exception("BL0007	- Biller Number already assigned to another account", new Exception("The biller number has already been assigned to another biller. Choose a different one"));
        //        }


        //        if (operationPaymentInstrumentId != 0)
        //        {
        //            if (context.PaymentInstruments.Count(x => x.PaymentInstrumentId == operationPaymentInstrumentId) < 1)
        //            {
        //                throw new Exception("PI0001	Payment Instrument '" + operationPaymentInstrumentId.ToString() + "' doesn’t exist");
        //            }
        //        }
        //        else
        //        {
        //            Customer customer = null;
        //            PaymentInstrument paymentInstrument = null;
        //            walletTransactionsRepository.MatchPaymentInstrumentToCustomer(biller.CustomerId, operationPaymentInstrumentId, out customer, out paymentInstrument);

        //            operationPaymentInstrumentId = paymentInstrument.PaymentInstrumentId;

        //        }

        //        if (overflowPaymentInstrumentId != 0)
        //        {
        //            if (context.PaymentInstruments.Count(x => x.PaymentInstrumentId == overflowPaymentInstrumentId) < 1)
        //            {
        //                throw new Exception(String.Format("PI0001	Payment Instrument '{0}' doesn’t exist", overflowPaymentInstrumentId));
        //            }
        //        }
        //        else
        //        {
        //            Customer customer = null;
        //            PaymentInstrument paymentInstrument = null;
        //            walletTransactionsRepository.MatchPaymentInstrumentToCustomer(biller.CustomerId, overflowPaymentInstrumentId, out customer, out paymentInstrument);

        //            overflowPaymentInstrumentId = paymentInstrument == null ? 0 : paymentInstrument.PaymentInstrumentId;

        //        }

        //        //operationPaymentInstrumentId =

        //        biller.BillerName = billerName;
        //        //biller.CustomerId = customerId;
        //        biller.BillerNumber = billerNumber;
        //        biller.SubsidizedAmount = subsidizedAmount;
        //        biller.TerminationDate = terminationDate;
        //        biller.SupportApplicationId = supportApplicationId;
        //        biller.BlackListReasonId = 1;
        //        biller.NotificationCustomerId = notificationCustomerId == 0 ? biller.CustomerId : notificationCustomerId;
        //        biller.BusinessLocation = businessLocation;
        //        biller.IsActive = isActive;
        //        biller.CreatedDate = DateTime.Now;
        //        biller.BillerTypeId = billerTypeId;
        //        biller.OperationPaymentInstrumentId = operationPaymentInstrumentId;
        //        biller.OverflowPaymentInstrumentId = overflowPaymentInstrumentId;
        //        biller.BillerEmailAddress = string.IsNullOrEmpty(emailAddress) ? null : emailAddress;
        //        biller.ShowInUI = showInUI;



        //        //BillerPaymentInstrument defaultBillerPaymentInstrument = new BillerPaymentInstrument
        //        //{
        //        //    BillerId = newBiller.BillerId,
        //        //    DefaultPaymentInstrumentId = _customerRepository.GetCustomerPaymentInstruments(customerId)[0].PaymentInstrumentId
        //        //};

        //        //  context.Billers.Add(newBiller);

        //        //context.BillerPaymentInstruments.Add(defaultBillerPaymentInstrument);
        //        context.SaveChanges();
        //        return biller;

        //    }
        //    catch (DbEntityValidationException dbEx)
        //    {
        //        foreach (var validationErrors in dbEx.EntityValidationErrors)
        //        {
        //            foreach (var validationError in validationErrors.ValidationErrors)
        //            {

        //                Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
        //                throw dbEx;
        //            }
        //            //throw dbEx;

        //        }
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    //}
        //}

        //public Biller UpdateBillerInformation(int billerId, string billerName, long subsidizedAmount, DateTime terminationDate, bool isActive)
        //{
        //    try
        //    {
        //        if (context.Billers.Count(x => x.BillerId == billerId) > 0)
        //        {
        //            Biller billerToUpdate = context.Billers.Single(x => x.BillerId == billerId);
        //            billerToUpdate.BillerName = billerName;
        //            billerToUpdate.SubsidizedAmount = subsidizedAmount;
        //            billerToUpdate.TerminationDate = terminationDate;
        //            billerToUpdate.IsActive = isActive;

        //            context.SaveChanges();
        //            return billerToUpdate;
        //        }
        //        else
        //        {
        //            throw new Exception("Biller Id does not exist", new Exception("No Biller with billerId: " + billerId + " exists."));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public BillerSettingConfiguration UpdateBillerSettingConfiguration(int billerSettingConfigurationId, string settingValue, string username, string password, string updatedByUsername, bool isActive)
        //{
        //    try
        //    {
        //        HelperRepository helperRepository = new HelperRepository();

        //        if (string.IsNullOrEmpty(updatedByUsername))
        //        {
        //            throw new Exception("CA0028 - Username must be set for this Action.");
        //        }

        //        if (string.IsNullOrEmpty(settingValue))
        //        {
        //            throw new Exception("BL0013 - Biller SettingValue must be set.");
        //        }

        //        helperRepository.ValidateUsername(updatedByUsername, false);

        //        BillerSettingConfiguration settingConfiguration = context.BillerSettingConfigurations.Find(billerSettingConfigurationId);

        //        if (settingConfiguration == null)
        //        {
        //            throw new Exception(String.Format("BL0018 - BillerSettingConfigurationId '{0}' does not exist.", billerSettingConfigurationId));
        //        }

        //        DateTime dateUpdated = DateTime.Now;


        //        ILoyal.DataLayer.Customer customer = context.Customers.Find(settingConfiguration.Biller.Customer.CustomerId);

        //        if (customer == null)
        //        {
        //            throw new Exception(String.Format("CA0004 - CustomerId '{0}' does not exist.", settingConfiguration.Biller.Customer.CustomerId));
        //        }

        //        string configurationKey = string.Empty;
        //        string configurationSecret = string.Empty;

        //        if (!string.IsNullOrEmpty(username))
        //        {
        //            configurationKey = EncDec.Encrypt(username, customer.Salt);
        //        }
        //        if (!string.IsNullOrEmpty(password))
        //        {
        //            configurationSecret = EncDec.Encrypt(password, customer.Salt);
        //        }


        //        settingConfiguration.SettingValue = settingValue;
        //        settingConfiguration.ConfigurationKey = string.IsNullOrEmpty(configurationKey) ? settingConfiguration.ConfigurationKey : configurationKey;
        //        settingConfiguration.ConfigurationSecret = string.IsNullOrEmpty(configurationSecret) ? settingConfiguration.ConfigurationSecret : configurationSecret;
        //        settingConfiguration.IsActive = isActive;
        //        //settingConfiguration.UpdateByUsername = updatedByUsername.ToLower();
        //        //settingConfiguration.UpdatedDate = dateUpdated;
        //        context.SaveChanges();

        //        return settingConfiguration;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        //       public Biller BillerIdExists(int billerId)
        //{
        //    Biller biller = context.Billers.Find(billerId);
        //    DateTime datumDate = DateTime.Now;

        //    if (biller == null)
        //    {
        //        throw new Exception(String.Format("BL0008 - The BillerId '{0}' does not exist.", billerId));
        //    }

        //    if (!biller.IsActive)
        //    {
        //        throw new Exception(String.Format("BL0009 - The BillerId '{0}' belongs to a biller that is inactive. No transactions are allowed for the biller.", billerId));
        //    }

        //    if (!biller.ServiceOnline)
        //    {
        //        throw new Exception(String.Format("BL0026 - The specified BillerId '{0}' is offline and cannot  participate in this transaction.", billerId));
        //    }

        //    if (datumDate > biller.TerminationDate)
        //    {
        //        EnforceBillerValidity(biller.BillerId);
        //        throw new Exception("BL0010 - The Biller is not active. The termination date is passed.");
        //    }

        //    return biller;
        //}

        //private bool BillerNumberAlreadySaved(long customerId, int billerId, int customerTypeId, out long savedBillerNumberId)
        //{
        //    savedBillerNumberId = 0;
        //    if (context.SavedBillerNumbers.Count(x => x.BillerId == billerId && x.CustomerId == customerId && x.CustomerTypeId == customerTypeId) > 0)
        //    {
        //        savedBillerNumberId = context.SavedBillerNumbers.Single(x => x.BillerId == billerId && x.CustomerId == customerId && x.CustomerTypeId == customerTypeId).SavedBillerNumberId;
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //private bool BillReferenceNumberAlreadySaved(long customerId, int billerId, int customerTypeId, string billReferenceNumber)
        //{
        //    if (context.SavedBillerNumbers.Count(x => x.BillerId == billerId && x.CustomerId == customerId && x.CustomerTypeId == customerTypeId) > 0)
        //    {
        //        SavedBillerNumber savedBillerNumber = context.SavedBillerNumbers.Single(x => x.BillerId == billerId && x.CustomerId == customerId && x.CustomerTypeId == customerTypeId);

        //        if (context.SavedBillReferenceNumbers.Any(x => x.SavedBillerNumberId == savedBillerNumber.SavedBillerNumberId && x.BillReferenceNumber == billReferenceNumber))
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //private Biller CreateSystemBiller(string billerName, string billerNumber, long msisdn)
        //{
        //    string userName = billerNumber.ToLower().Replace(" ", string.Empty).Replace("-", string.Empty);

        //    if (!string.IsNullOrEmpty(userName))
        //    {
        //        ILoyal.DataLayer.Customer existingcustomer = _customerRepository.GetCustomerByUsername(userName);

        //        if (existingcustomer != null)
        //        {
        //            throw new Exception(String.Format("CA0013	- The Username '{0}' has already been taken", userName));
        //        }
        //    }

        //    ILoyal.DataLayer.Customer billerCustomer = new ILoyal.DataLayer.Customer
        //    {
        //        CustomerTypeId = (int)CustomerTypes.SystemUser,
        //        RegisteredByUsername = "system",
        //        FirstName = billerName,
        //        LastName = null,
        //        MiddleName = null,
        //        LanguageId = applicationSettings.DefaultInstanceLanguage,
        //        FullyRegistered = true,
        //        EmailAddress = null,
        //        InformationModeId = 1,
        //        IsBlacklisted = false,
        //        IsTestCustomer = false,
        //        IdNumber = null,
        //        IdTypeId = 1,
        //        AccessChannelId = 1,
        //        CountryId = applicationSettings.DefaultCountry,
        //        CountyId = 1,
        //        SecurityCode = string.Empty,
        //        LoginAttempts = 0,
        //        UserLoggedIn = false,
        //        TaxNumber = null,
        //        TermsAccepted = false,
        //        TermsAcceptedDate = null,
        //        ApplicationId = 1,
        //        DeactivatedAccount = true,
        //        CreatedDate = DateTime.Now,
        //        Nonce = null,
        //        UserName = billerNumber.ToLower().Replace(" ", string.Empty).Replace("-", string.Empty),
        //        Salt = randomStringGenerator.NextString(256, true, true, true, true),
        //    };
        //    ILoyal.DataLayer.Customer customer = _customerRepository.CreateNewCustomer(billerCustomer, false, 1, 1, msisdn, true, true);

        //    Biller biller = null;

        //    if (customer != null)
        //    {
        //        SystemUser systemUser = new SystemUser
        //        {
        //            CustomerId = customer.CustomerId,
        //            ShowInResults = false
        //        };
        //        context.SystemUsers.Add(systemUser);
        //        context.SaveChanges();

        //        //TODO remove the hard coded Nairobi below
        //        biller = CreateBiller(1, customer.CustomerId, customer.CustomerId, (int)BillerTypes.Corporate, billerName, billerNumber, string.Empty, "Nairobi", 0, DateTime.MaxValue, 0, 0, true, false);
        //    }
        //    else
        //    {
        //        throw new Exception("Customer Was not created Successfully");
        //    }

        //    return biller;
        //}

        //private int GetExponentialBackoffSlotTime(int frequencyOfRetry)
        //{
        //    //implement n+1 as the factor to determine which time slot to pick
        //    try
        //    {
        //        int retryInterval = 0;

        //        if (frequencyOfRetry >= 0 && frequencyOfRetry < 10)
        //        {
        //            retryInterval = context.IPNExponentialBackOffs.Find(frequencyOfRetry + 1).SlotTime;
        //        }

        //        return retryInterval;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //}

        //public long GetInBillRepoCustomerMsisdn(long customerId)
        //{
        //    try
        //    {
        //        CustomerMsisdn convertedMsisdn = context.CustomerMsisdns.SingleOrDefault(x => x.CustomerId == customerId);

        //        if (convertedMsisdn == null)
        //        {
        //            throw new Exception(String.Format("CA0004 -  CustomerId {0} does not exist.", customerId));
        //        }

        //        return context.MasterMsisdnLogs.First(x => x.MasterMsisdnLogId == convertedMsisdn.MasterMsidnLogId).Msisdn;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public Biller ToggleBillerSettings(int billerId, bool isServiceOnline, bool isActive, bool notifyCustomersOnResume, bool earnAirtimeCommission)
        //{
        //    Biller biller = context.Billers.Find(billerId);

        //    if (biller == null)
        //    {
        //        throw new Exception(String.Format("BL0008 - The BillerId '{0}' does not exist.", billerId));
        //    }

        //    biller.ServiceOnline = isServiceOnline;
        //    biller.IsActive = isActive;
        //    biller.NotifyCustomersOnResume = notifyCustomersOnResume;
        //    biller.EarnAirtimeCommission = earnAirtimeCommission;

        //    context.SaveChanges();
        //    return biller;
        //}

        //public Biller SetBillerServiceAvailabilityStatus(int billerId, bool isServiceOnline)
        //{
        //    Biller biller = context.Billers.Find(billerId);

        //    if (biller == null)
        //    {
        //        throw new Exception(String.Format("BL0008 - The BillerId '{0}' does not exist.", billerId));
        //    }

        //    biller.ServiceOnline = isServiceOnline;

        //    context.SaveChanges();
        //    return biller;
        //}

    }
}