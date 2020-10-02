using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Data.SqlClient;

using Dapper;

using Hook.Enums;
using Hook.Models;
using Microsoft.Ajax.Utilities;
using Hook.Helper;

namespace Hook.Repository
{
    public class HelperRepository
    {
        private string sqlConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString();
        static TransactionRepository transactionRepository = new TransactionRepository();
        CustomerRepository customerRepository = new CustomerRepository();
        static RandomStringGenerator pinGenerator = new RandomStringGenerator();

        WalletTransactionsRepository walletTransactionRepository = new WalletTransactionsRepository();

        Dictionary<string, string> ravenParameters = new Dictionary<string, string>();
        const string emailCopyList = "info@hook.com";


        public static string GenerateTransactionReferenceNumber()
        {
            return pinGenerator.NextString(6, false, true, true, false);
        }

        public void ValidateUsername(string username, bool allowSystemUser)
        {
            Customer registeringCustomer = customerRepository.GetCustomerByUsername(username);

            if (registeringCustomer == null)
            {
                throw new Exception(string.Format("CA0011 - The Username '{0}' does not exist.", username));
            }
            if (!allowSystemUser)
            {
                if (customerRepository.GetCustomerByUsername(username).UserName == "system")
                {
                    throw new Exception(string.Format("CA0024 - The system Username '{0}' is not allowed to perform this operation", username));
                }
            }
        }

        public TransactionType GetTransactionTypeByTransactionTypeId(long transactionTypeId)
        {
            TransactionType transactionType = null;
            try
            {
                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();

                    transactionType = connection.Query<TransactionType>("SELECT * FROM TransactionType WHERE TransactionTypeId=@TransactionTypeId", new { TransactionTypeId = transactionTypeId }).SingleOrDefault();

                    connection.Close();
                }

                if (transactionType == null)
                    throw new Exception(string.Format("TRN0024 - The specified TransactionTypeId '{0}' does not exist.", transactionTypeId));

                return transactionType;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool CustomerHasSufficientFundsForTransaction(decimal customerBalance, long transactionAmount, long transactionFee)
        {
            if (Convert.ToInt64(customerBalance) < (transactionAmount + transactionFee))
            {
                throw new Exception("TRN0005 - Customer has in insufficient funds for the transaction");
                //return false;
            }
            return true;
        }

        public static bool UpdateCustomerSVABalance(PaymentInstrument customerPaymentInstrument, long amount, long fee, bool creditAccount)
        {
            try
            {
                long accountBalance = 0;

                if (customerPaymentInstrument != null)
                {

                    if (customerPaymentInstrument.IsMobileWallet)
                    {
                        if (creditAccount)
                        {
                            accountBalance = customerPaymentInstrument.AccountBalance += amount;
                        }
                        else
                        {
                            accountBalance = customerPaymentInstrument.AccountBalance -= (amount + fee);
                        }

                        PaymentInstrument paymentInstrument = transactionRepository.UpdatePaymentInstrument(accountBalance, customerPaymentInstrument.CustomerId);
                    }
                }
                else
                {
                    throw new Exception("PI0015 - Customer payment instrument is null: " + customerPaymentInstrument.PaymentInstrumentId.ToString());
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool UpdateBillerSVABalance(Biller redemptionBiller, long amount, string transactionReference, bool creditAccount, string narrative)
        {
            try
            {
                PaymentInstrument paymentInstrument = null;
                long accountBalance = 0;

                paymentInstrument = transactionRepository.GetPaymentInstrumentByCustomerId(redemptionBiller.CustomerId);

                if (paymentInstrument.PaymentInstrumentTypeId == (int)PITypes.Voucher || paymentInstrument.PaymentInstrumentTypeId == (int)PITypes.MPESA || paymentInstrument.PaymentInstrumentTypeId == (int)PITypes.LoyaltySVA || paymentInstrument.PaymentInstrumentTypeId == (int)PITypes.CreditCard)
                {
                    if (creditAccount)
                    {
                        accountBalance = paymentInstrument.AccountBalance += amount;
                    }
                    else
                    {
                        accountBalance = paymentInstrument.AccountBalance -= amount;
                    }

                    paymentInstrument = transactionRepository.UpdatePaymentInstrument(accountBalance, redemptionBiller.CustomerId);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static bool UpdateSVABalance(PaymentInstrument currentPaymentInstrument, long netAmount, bool creditAccount)
        {
            try
            {
                if (currentPaymentInstrument != null)
                {
                    long accountBalance = 0;

                    if (currentPaymentInstrument.IsMobileWallet)
                    {
                        if (creditAccount)
                        {
                            accountBalance = currentPaymentInstrument.AccountBalance += netAmount;
                        }
                        else
                        {
                            accountBalance = currentPaymentInstrument.AccountBalance -= netAmount;
                        }

                        PaymentInstrument paymentInstrument = transactionRepository.UpdatePaymentInstrument(accountBalance, currentPaymentInstrument.CustomerId);
                    }
                }
                else
                {
                    throw new Exception(string.Format("PI0015 - Customer payment instrument is null: {0}", currentPaymentInstrument.PaymentInstrumentId));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void UpdateCumulativeCustomerTransactionLimit(long customerId, PaymentInstrument paymentInstrument, long amount)
        {
            CumulativeCustomerTransactionAmount cumulativeCustomerTransactionAmount = null;

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                cumulativeCustomerTransactionAmount = connection.Query<CumulativeCustomerTransactionAmount>("SELECT * FROM CumulativeCustomerTransactionAmount WHERE CustomerId=@CustomerId AND PaymentInstrumentId=@PaymentInstrumentId", new { CustomerId = customerId, PaymentInstrumentId = paymentInstrument.PaymentInstrumentId }).SingleOrDefault();

                connection.Close();
            }


            DateTime transactionDate = transactionRepository.GetRealDate();

            if (cumulativeCustomerTransactionAmount == null)
            {
                CumulativeCustomerTransactionAmount customerTransactionAmount = new CumulativeCustomerTransactionAmount
                {
                    CustomerId = customerId,
                    PaymentInstrumentId = paymentInstrument.PaymentInstrumentId,
                    CumulativeDailyAmount = amount,
                    CumulativeMonthlyAmount = amount,
                    TransactionMonth = transactionDate.Month,
                    TransactionDate = transactionDate
                };

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    var affectedRows = connection.Execute("INSERT INTO CumulativeCustomerTransactionAmount (CustomerId, PaymentInstrumentId, CumulativeDailyAmount,CumulativeMonthlyAmount,TransactionMonth,TransactionDate) VALUES (@CustomerId,@PaymentInstrumentId,@CumulativeDailyAmount,@CumulativeMonthlyAmount,@TransactionMonth,@TransactionDate)", new { customerTransactionAmount.CustomerId, customerTransactionAmount.PaymentInstrumentId, customerTransactionAmount.CumulativeDailyAmount, customerTransactionAmount.CumulativeMonthlyAmount, customerTransactionAmount.TransactionMonth, customerTransactionAmount.TransactionDate });

                    connection.Close();
                }
            }
            else
            {
                if (cumulativeCustomerTransactionAmount.TransactionMonth.Equals(transactionDate.Month))
                {
                    cumulativeCustomerTransactionAmount.CumulativeMonthlyAmount += amount;
                }
                else
                {
                    cumulativeCustomerTransactionAmount.TransactionMonth = transactionDate.Month;
                    cumulativeCustomerTransactionAmount.CumulativeMonthlyAmount = amount;
                }

                if (cumulativeCustomerTransactionAmount.TransactionDate.ToShortDateString().Equals(transactionDate.ToShortDateString()))
                {
                    cumulativeCustomerTransactionAmount.CumulativeDailyAmount += amount;
                }
                else
                {
                    cumulativeCustomerTransactionAmount.TransactionDate = transactionDate;
                    cumulativeCustomerTransactionAmount.CumulativeDailyAmount = amount;
                }

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    var affectedRows = connection.Execute("UPDATE CumulativeCustomerTransactionAmount SET CumulativeMonthlyAmount=@CumulativeMonthlyAmount,TransactionMonth=@TransactionMonth,CumulativeDailyAmount=@CumulativeDailyAmount,TransactionDate=@TransactionDate WHERE CustomerId = @CustomerId AND PaymentInstrumentId=@PaymentInstrumentId", new { CumulativeMonthlyAmount = cumulativeCustomerTransactionAmount.CumulativeMonthlyAmount, TransactionMonth = cumulativeCustomerTransactionAmount.TransactionMonth, CumulativeDailyAmount = cumulativeCustomerTransactionAmount.CumulativeDailyAmount, TransactionDate = cumulativeCustomerTransactionAmount.TransactionDate, CustomerId = customerId, PaymentInstrumentId = paymentInstrument.PaymentInstrumentId });

                    connection.Close();

                }
            }

        }

    }
}