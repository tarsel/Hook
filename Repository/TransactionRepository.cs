using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Data.SqlClient;

using Dapper;

using Hook.Models;
using Hook.Helper;
using Hook.Enums;

namespace Hook.Repository
{
    public class TransactionRepository
    {
        string sqlConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString();

        RandomStringGenerator randomStringGenerator = new RandomStringGenerator();

        private LoyaltyRepository loyaltyRepository = new LoyaltyRepository();

        public bool AirtimePurchase(long amount, long msisdn)
        {
            return true;
        }

        public CustomerLoyaltyPoint BuyAirtime(int organizationId, long paymentInstrumentId, long amount, long msisdn)
        {
            try
            {
                if (AirtimePurchase(amount, msisdn) == true)
                {
                    // Customer customer = customerRepository.GetCustomerByMsisdn(msisdn);
                    return loyaltyRepository.CreatePoints(organizationId, paymentInstrumentId, amount);
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public CustomerLoyaltyPoint SellAirtime(int organizationId, long paymentInstrumentId, long amount, long msisdn)
        {
            try
            {
                if (AirtimePurchase(amount, msisdn) == true)
                {
                    return loyaltyRepository.CreatePoints(organizationId, paymentInstrumentId, amount);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public MasterTransactionRecord RedeemPoints(int organizationId, long customerId, long paymentInstrumentId, long pointsToRedeem)
        {
            try
            {
                return loyaltyRepository.RedeemLoyaltyPoints(organizationId, customerId, paymentInstrumentId, pointsToRedeem);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public CustomerLoyaltyPoint CheckPointsBalance(int organizationId, long customerId, long paymentInstrumentId)
        {
            try
            {
                return loyaltyRepository.GetCustomerLoyaltyPointsByPaymentInstrument(organizationId, customerId, paymentInstrumentId);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public LoyaltyPointExecutionBot TransferPoints(int organizationId, long sourceCustomerId, long sourcePaymentInstrumentId, long destinationCustomerId, long destinationPaymentInstrumentId, long pointsToTransfer)
        {
            try
            {
                return loyaltyRepository.TransferingLoyaltyPoints(organizationId, sourceCustomerId, sourcePaymentInstrumentId, destinationCustomerId, destinationPaymentInstrumentId, pointsToTransfer, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public ReverseTransactionResult ReversePoints(long loyaltyPointExecutionBotId, string reversedByUsername)
        {
            try
            {
                return loyaltyRepository.ReverseTransferedLoyaltyPoints(loyaltyPointExecutionBotId, reversedByUsername);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int CreateTransactionType(string transactionTypeName, string friendlyName, long amount)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("INSERT INTO TransactionType (TransactionTypeName, FriendlyName, Amount) VALUES (@TransactionTypeName, @FriendlyName, @Amount)", new { transactionTypeName, friendlyName, amount });

                connection.Close();

                return affectedRows;
            }
        }

        public TransactionType UpdateTransactionType(long transactionTypeId, string transactionTypeName, string friendlyName, long amount)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("UPDATE TransactionType SET TransactionTypeName=@TransactionTypeName, FriendlyName=@FriendlyName, Amount=@Amount WHERE TransactionTypeId = @TransactionTypeId", new { TransactionTypeId = transactionTypeId, TransactionTypeName = transactionTypeName, FriendlyName = friendlyName, Amount = amount });

                connection.Close();

                return GetTransactionTypeByTransactionTypeId(transactionTypeId);
            }
        }

        public TransactionType GetTransactionTypeByTransactionTypeId(long transactionTypeId)
        {
            TransactionType transactionType = new TransactionType();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                transactionType = connection.Query<TransactionType>("SELECT * FROM TransactionType WHERE TransactionTypeId=@TransactionTypeId", new { TransactionTypeId = transactionTypeId }).SingleOrDefault();

                connection.Close();
            }

            return transactionType;
        }


        public int MakeTransaction(long customerId, long transactionTypeId, long transactionAmount, long customerTypeId)
        {
            try
            {
                PaymentInstrument payerPi = GetPaymentInstrumentByCustomerId(customerId);

                if (payerPi.AccountBalance < transactionAmount)
                {
                    throw new Exception("Sorry! You do not have sufficient Balance to make this transaction");
                }
                else
                {
                    Customer payeeCustomer = new CustomerRepository().GetCustomerByUsername("walletuser");

                    PaymentInstrument payeePi = GetPaymentInstrumentByCustomerId(payeeCustomer.CustomerId);

                    MasterTransactionRecord newTransaction = new MasterTransactionRecord
                    {
                        TransactionTypeId = transactionTypeId,
                        PayerId = customerId,
                        PayeeId = payeeCustomer.CustomerId,
                        PayerPaymentInstrumentId = payerPi.PaymentInstrumentId,
                        PayeePaymentInstrumentId = payeePi.PaymentInstrumentId,
                        AccessChannelId = 1,
                        Amount = transactionAmount,
                        Fee = transactionAmount,//Should be the amount payable to the vendor
                        CustomerTypeId = customerTypeId,
                        Tax = 0,
                        TransactionErrorCodeId = 1,
                        IsTestTransaction = false,
                        TransactionDate = GetRealDate(),
                        TransactionReference = "HK" + randomStringGenerator.NextTokenString(7, true, true, true, false),
                        PayerBalanceBeforeTransaction = payerPi.AccountBalance,
                        PayeeBalanceBeforeTransaction = payeePi.AccountBalance,
                        TransactionStatusId = TransactionState.Successful,
                        PayeeBalanceAfterTransaction = (payeePi.AccountBalance + transactionAmount),
                        PayerBalanceAfterTransaction = (payerPi.AccountBalance - transactionAmount),
                        ReversedTransactionOriginalTypeId = 1,
                    };

                    using (var connection = new SqlConnection(sqlConnectionString))
                    {
                        connection.Open();

                        var affectedRows = connection.Execute("INSERT INTO MasterTransactionRecord (PayerId, PayerPaymentInstrumentId, PayeeId, PayeePaymentInstrumentId, TransactionReference, TransactionTypeId, TransactionErrorCodeId, AmountPaid, Fee, Tax, TransactionDate, CustomerTypeId, PayerBalanceBeforeTransaction, PayerBalanceAfterTransaction, PayeeBalanceBeforeTransaction, PayeeBalanceAfterTransaction, IsTestTransaction, AccessChannelId, SourceUserName, DestinationUserName, TransactionStatusId, ThirdPartyTransactionId,  TransactionTypeCategoryId,ReversedTransactionOriginalTypeId,KeyIdentifier,TownId,SubcountyId,IdNumber, ZoneId) VALUES (@PayerId, @PayerPaymentInstrumentId, @PayeeId, @PayeePaymentInstrumentId, @TransactionReference, @TransactionTypeId, @TransactionErrorCodeId, @AmountPaid, @Fee, @Tax, @TransactionDate, @CustomerTypeId, @PayerBalanceBeforeTransaction, @PayerBalanceAfterTransaction, @PayeeBalanceBeforeTransaction, @PayeeBalanceAfterTransaction, @IsTestTransaction, @AccessChannelId, @SourceUserName, @DestinationUserName, @TransactionStatusId,@ThirdPartyTransactionId,@ReversedTransactionOriginalTypeId)", new { newTransaction.PayerId, newTransaction.PayerPaymentInstrumentId, newTransaction.PayeeId, newTransaction.PayeePaymentInstrumentId, newTransaction.TransactionReference, newTransaction.TransactionTypeId, newTransaction.TransactionErrorCodeId, newTransaction.Amount, newTransaction.Fee, newTransaction.Tax, newTransaction.TransactionDate, newTransaction.CustomerTypeId, newTransaction.PayerBalanceBeforeTransaction, newTransaction.PayerBalanceAfterTransaction, newTransaction.PayeeBalanceBeforeTransaction, newTransaction.PayeeBalanceAfterTransaction, newTransaction.IsTestTransaction, newTransaction.AccessChannelId, newTransaction.SourceUserName, newTransaction.DestinationUserName, newTransaction.TransactionStatusId, newTransaction.ThirdPartyTransactionId, newTransaction.ReversedTransactionOriginalTypeId, newTransaction.ReversedTransactionOriginalTypeId.Value });

                        connection.Close();

                        if (affectedRows == 1)
                        {
                            PaymentInstrument updatePayerNewBal = UpdatePaymentInstrument(newTransaction.PayerBalanceAfterTransaction.Value, payerPi.CustomerId);
                            PaymentInstrument updatePayeeNewBal = UpdatePaymentInstrument(newTransaction.PayeeBalanceAfterTransaction.Value, payeePi.CustomerId);

                            TransactionTypeCategory type = GetTransactionTypeCategoryByTransactionTypeId(transactionTypeId).FirstOrDefault();

                            TransactionType transactionType = GetTransactionTypeByTransactionTypeId(transactionTypeId);

                            //string docNumber = "BC" + Guid.NewGuid().ToString().Replace('-', '.').Take(6);
                            // Town town = new SetUpRepository().GetTownByTownId(townId);

                            //new ReceiptRepository().SendPDFEmail(town.TownName, transactionType.TransactionTypeName, keyIdentifier, type.Amount.ToString(), newTransaction.TransactionReference, type.TransactionTypeCategoryName, newTransaction.TransactionReference, idNumber, email);
                        }

                        return affectedRows;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public MasterTransactionRecord GetTransactionByMasterTransactionId(long masterTransactionRecordId)
        {
            MasterTransactionRecord record = new MasterTransactionRecord();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                record = connection.Query<MasterTransactionRecord>("SELECT * FROM MasterTransactionRecord WHERE MasterTransactionRecordId=@MasterTransactionRecordId", new { MasterTransactionRecordId = masterTransactionRecordId }).SingleOrDefault();

                connection.Close();
            }

            return record;
        }

        public List<MasterTransactionRecord> GetAllDefaultTransactions()
        {
            List<MasterTransactionRecord> record = new List<MasterTransactionRecord>();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                record = connection.Query<MasterTransactionRecord>("SELECT * FROM MasterTransactionRecord ORDER BY TransactionDate Desc").ToList();

                connection.Close();
            }

            return record;
        }

        public List<MinifiedTransactionRecord> GetAllTransactions()
        {
            List<MinifiedTransactionRecord> record = new List<MinifiedTransactionRecord>();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                record = connection.Query<MinifiedTransactionRecord>("SELECT c.FirstName, c.LastName, c.IdNumber, b.TransactionTypeName, a.AmountPaid, a.PayerBalanceBeforeTransaction, a.PayerBalanceAfterTransaction, d.TownName, e.SubCountyName, a.TransactionDate, f.TransactionTypeCategoryName FROM TransactionType b, MasterTransactionRecord a, Customer c, Town d, SubCounty e, TransactionTypeCategory f WHERE b.TransactionTypeId = a.TransactionTypeId AND c.CustomerId = a.PayerId AND f.TransactionTypeCategoryId = a.TransactionTypeCategoryId ORDER BY a.TransactionDate Desc").ToList();

                connection.Close();
            }

            return record;
        }

        public int CreatePaymentInstrumentType(string paymentInstrumentTypeName)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("INSERT INTO PaymentInstrumentType (PaymentInstrumentTypeName) VALUES (@PaymentInstrumentTypeName)", new { paymentInstrumentTypeName });

                connection.Close();

                return affectedRows;
            }
        }
        public PaymentInstrumentType UpdatePaymentInstrumentType(long paymentInstrumentTypeId, string paymentInstrumentTypeName)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                var affectedRows = connection.Execute("UPDATE PaymentInstrumentType SET PaymentInstrumentTypeName=@PaymentInstrumentTypeName WHERE PaymentInstrumentTypeId=@PaymentInstrumentTypeId", new { PaymentInstrumentTypeId = paymentInstrumentTypeId, PaymentInstrumentTypeName = paymentInstrumentTypeName });

                connection.Close();

                return GetPaymentInstrumentTypeById(paymentInstrumentTypeId);
            }
        }
        public PaymentInstrumentType GetPaymentInstrumentTypeById(long paymentInstrumentTypeId)
        {
            PaymentInstrumentType pIType = new PaymentInstrumentType();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                pIType = connection.Query<PaymentInstrumentType>("SELECT * FROM PaymentInstrumentType WHERE PaymentInstrumentTypeId=@PaymentInstrumentTypeId", new { PaymentInstrumentTypeId = paymentInstrumentTypeId }).SingleOrDefault();

                connection.Close();
            }

            return pIType;
        }

        public PaymentInstrument CreatePaymentInstrument(long customerId)
        {
            try
            {
                PaymentInstrument newPaymentInstrument = new PaymentInstrument
                {
                    AccountBalance = 0,
                    AccountNumber = "HK Wallet",
                    AllowCredit = true,
                    AllowDebit = true,
                    CustomerId = customerId,
                    DateLinked = GetRealDate(),
                    DateVerified = GetRealDate(),
                    IsDefaultFIAccount = false,
                    IsMobileWallet = true,
                    PaymentInstrumentTypeId = 1,
                    IsSuspended = false,
                    PaymentIntrumentAlias = "HK Wallet",
                    IsActive = true,
                    LoyaltyPointBalance = 0,
                    Verified = true
                };

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    var affectedRows = connection.Execute("INSERT INTO PaymentInstrument (AccountBalance, AccountNumber, AllowCredit, AllowDebit, CustomerId, DateLinked, DateVerified, IsDefaultFIAccount, IsMobileWallet, PaymentInstrumentTypeId, IsSuspended, PaymentIntrumentAlias, IsActive,LoyaltyPointBalance,Verified) VALUES (@AccountBalance, @AccountNumber, @AllowCredit, @AllowDebit, @CustomerId, @DateLinked, @DateVerified, @IsDefaultFIAccount, @IsMobileWallet, @PaymentInstrumentTypeId, @IsSuspended, @PaymentIntrumentAlias, @IsActive,@LoyaltyPointBalance,@Verified)", new { newPaymentInstrument.AccountBalance, newPaymentInstrument.AccountNumber, newPaymentInstrument.AllowCredit, newPaymentInstrument.AllowDebit, newPaymentInstrument.CustomerId, newPaymentInstrument.DateLinked, newPaymentInstrument.DateVerified, newPaymentInstrument.IsDefaultFIAccount, newPaymentInstrument.IsMobileWallet, newPaymentInstrument.IsSuspended, newPaymentInstrument.PaymentIntrumentAlias, newPaymentInstrument.IsActive, newPaymentInstrument.PaymentInstrumentTypeId, newPaymentInstrument.LoyaltyPointBalance, newPaymentInstrument.Verified });

                    connection.Close();
                }

                return GetPaymentInstrumentByCustomerId(customerId);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public PaymentInstrument GetPaymentInstrumentByCustomerId(long customerId)
        {
            PaymentInstrument pi = new PaymentInstrument();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                pi = connection.Query<PaymentInstrument>("SELECT * FROM PaymentInstrument WHERE CustomerId=@CustomerId", new { CustomerId = customerId }).SingleOrDefault();

                connection.Close();
            }

            return pi;
        }

        public PaymentInstrument GetPaymentInstrumentByPaymentInstrumentId(long paymentInstrumentId)
        {
            PaymentInstrument pi = new PaymentInstrument();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                pi = connection.Query<PaymentInstrument>("SELECT * FROM PaymentInstrument WHERE PaymentInstrumentId=@PaymentInstrumentId", new { PaymentInstrumentId = paymentInstrumentId }).SingleOrDefault();

                connection.Close();
            }

            return pi;
        }
        public PaymentInstrument UpdateLoyaltyPaymentInstrumentByPaymentInstrumentId(long paymentInstrumentId, long loyaltyPointBal)
        {
            PaymentInstrument pi = new PaymentInstrument();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                pi = connection.Query<PaymentInstrument>("UPDATE PaymentInstrument SET LoyaltyPointBalance = @LoyaltyPointBalance WHERE PaymentInstrumentId=@PaymentInstrumentId", new { LoyaltyPointBalance = loyaltyPointBal, PaymentInstrumentId = paymentInstrumentId }).SingleOrDefault();

                connection.Close();
            }

            return pi;
        }

        public List<PaymentInstrument> GetAllPaymentInstruments()
        {
            List<PaymentInstrument> pi = new List<PaymentInstrument>();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                pi = connection.Query<PaymentInstrument>("SELECT * FROM PaymentInstrument").ToList();

                connection.Close();
            }

            return pi;
        }

        public List<PaymentInstrumentType> GetAllPaymentInstrumentTypes()
        {
            List<PaymentInstrumentType> pIType = new List<PaymentInstrumentType>();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                pIType = connection.Query<PaymentInstrumentType>("SELECT * FROM PaymentInstrumentType").ToList();

                connection.Close();
            }

            return pIType;
        }

        public List<TransactionType> GetAllTransactionTypes()
        {
            List<TransactionType> transactionType = new List<TransactionType>();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                transactionType = connection.Query<TransactionType>("SELECT * FROM TransactionType").ToList();

                connection.Close();
            }

            return transactionType;
        }

        public List<MasterTransactionRecord> GetTransactionsByWard(long wardId)
        {
            List<MasterTransactionRecord> pi = new List<MasterTransactionRecord>();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                pi = connection.Query<MasterTransactionRecord>("SELECT * FROM MasterTransactionRecord WHERE TownId=@TownId", new { TownId = wardId }).ToList();

                connection.Close();
            }

            return pi;
        }

        public List<MasterTransactionRecord> GetTransactionsByDate(DateTime startDate, DateTime endDate)
        {
            List<MasterTransactionRecord> pi = new List<MasterTransactionRecord>();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                pi = connection.Query<MasterTransactionRecord>("SELECT * FROM MasterTransactionRecord WHERE TransactionDate>=@StartDate AND TransactionDate<=@EndDate", new { StartDate = startDate, EndDate = endDate }).ToList();

                connection.Close();
            }

            return pi;
        }
        public List<MasterTransactionRecord> GetTransactionsByMsisdn(long msisdn)
        {
            Customer cust = new CustomerRepository().GetCustomerByMsisdn(msisdn);

            List<MasterTransactionRecord> pi = new List<MasterTransactionRecord>();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                pi = connection.Query<MasterTransactionRecord>("SELECT * FROM MasterTransactionRecord WHERE CustomerId=@CustomerId", new { cust.CustomerId }).ToList();

                connection.Close();
            }

            return pi;
        }

        public List<MasterTransactionRecord> GetTransactionsByIdNumber(string idNumber)
        {
            List<MasterTransactionRecord> pi = new List<MasterTransactionRecord>();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                pi = connection.Query<MasterTransactionRecord>("SELECT * FROM MasterTransactionRecord WHERE IdNumber=@IdNumber", new { IdNumber = idNumber }).ToList();

                connection.Close();
            }

            return pi;
        }

        public List<MasterTransactionRecord> GetTransactionsByTransactionType(long transactionTypeId)
        {
            List<MasterTransactionRecord> pi = new List<MasterTransactionRecord>();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                pi = connection.Query<MasterTransactionRecord>("SELECT * FROM MasterTransactionRecord WHERE TransactionTypeId=@TransactionTypeId", new { transactionTypeId }).ToList();

                connection.Close();
            }

            return pi;
        }

        public PaymentInstrument UpdatePaymentInstrument(long accountBalance, long customerId)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                var affectedRows = connection.Execute("UPDATE PaymentInstrument SET AccountBalance=@AccountBalance WHERE CustomerId=@CustomerId", new { AccountBalance = accountBalance, CustomerId = customerId });

                connection.Close();

                return GetPaymentInstrumentByCustomerId(customerId);
            }
        }

        public List<TransactionReport> GetTransactionsReport()
        {
            List<TransactionReport> transactionReport = new List<TransactionReport>();

            string querry = "SELECT MasterTransactionRecordId,FirstName,IdNumber,Msisdn,TransactionReference,TransactionTypeName,TransactionDate,PayerBalanceBeforeTransaction,PayerBalanceAfterTransaction,AmountPaid FROM MasterTransactionRecord,Customer,TransactionType";

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                transactionReport = connection.Query<TransactionReport>(querry).ToList();

                connection.Close();
            }
            return transactionReport;
        }

        public int CreateTransactionTypeCategory(long transactionTypeId, string transactionTypeCategoryName, long Amount)
        {

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("INSERT INTO TransactionTypeCategory (TransactionTypeCategoryName, TransactionTypeId, Amount) VALUES (@TransactionTypeCategoryName, @TransactionTypeId, @Amount)", new { transactionTypeCategoryName, transactionTypeId, Amount });

                connection.Close();

                return affectedRows;
            }
        }

        public TransactionTypeCategory UpdateTransactionTypeCategory(long transactionTypeCategoryId, long transactionTypeId, string transactionTypeCategoryName, long Amount)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                var affectedRows = connection.Execute("UPDATE TransactionTypeCategory SET TransactionTypeCategoryName=@TransactionTypeCategoryName, TransactionTypeId=@TransactionTypeId, Amount=@Amount WHERE TransactionTypeCategoryId=@TransactionTypeCategoryId", new { transactionTypeCategoryName, transactionTypeId, Amount, transactionTypeCategoryId });

                connection.Close();

                return GetTransactionTypeCategoryByCategoryId(transactionTypeCategoryId);
            }
        }

        public TransactionTypeCategory GetTransactionTypeCategoryByCategoryId(long transactionTypeCategoryId)
        {
            TransactionTypeCategory transactionTypeCategory = new TransactionTypeCategory();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                transactionTypeCategory = connection.Query<TransactionTypeCategory>("SELECT * FROM TransactionTypeCategory WHERE TransactionTypeCategoryId=@TransactionTypeCategoryId", new { transactionTypeCategoryId }).SingleOrDefault();

                connection.Close();
            }

            return transactionTypeCategory;
        }

        public List<TransactionTypeCategory> GetAllTransactionTypeCategory()
        {
            List<TransactionTypeCategory> transactionType = new List<TransactionTypeCategory>();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                transactionType = connection.Query<TransactionTypeCategory>("SELECT * FROM TransactionTypeCategory").ToList();

                connection.Close();
            }

            return transactionType;
        }

        public List<TransactionTypeCategory> GetTransactionTypeCategoryByTransactionTypeId(long transactionTypeId)
        {
            List<TransactionTypeCategory> transactionTypeCategory = new List<TransactionTypeCategory>();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                transactionTypeCategory = connection.Query<TransactionTypeCategory>("SELECT * FROM TransactionTypeCategory WHERE TransactionTypeId=@TransactionTypeId", new { transactionTypeId }).ToList();

                connection.Close();
            }

            return transactionTypeCategory;
        }

        public DateTime GetRealDate()
        {
            //Set the time zone information to E. Africa Standard Time 
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time");
            //Get date and time in US Mountain Standard Time 
            DateTime dateTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);

            return dateTime;
        }

    }
}