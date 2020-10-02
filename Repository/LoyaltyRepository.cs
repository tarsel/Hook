using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

using Dapper;

using Hook.Enums;
using Hook.Models;


namespace Hook.Repository
{
    public class LoyaltyRepository
    {
        string sqlConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString();

      //  private long initialTransactionFrequency = 1;
        private int baseCurrencyConversionUnit = 100;


        public CustomerLoyaltyPoint GetCustomerLoyaltyPointsByPaymentInstrument(int organizationId, long customerId, long paymentInstrumentId)
        {
            try
            {
                CustomerLoyaltyPoint customerLoyaltyPoint = new CustomerLoyaltyPoint();

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();

                    customerLoyaltyPoint = connection.Query<CustomerLoyaltyPoint>("SELECT * FROM CustomerLoyaltyPoint WHERE OrganizationId=@OrganizationId AND PaymentInstrumentId=@PaymentInstrumentId AND CustomerId=@CustomerId", new { OrganizationId = organizationId, PaymentInstrumentId = paymentInstrumentId, CustomerId = customerId }).SingleOrDefault();

                    connection.Close();
                }

                return customerLoyaltyPoint;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public CustomerLoyaltyPoint GetCustomerLoyaltyPointByMinified(int organizationId, long paymentInstrumentId)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                CustomerLoyaltyPoint customerLoyaltyPoint = connection.Query<CustomerLoyaltyPoint>("SELECT * FROM CustomerLoyaltyPoint WHERE OrganizationId=@OrganizationId AND PaymentInstrumentId=@PaymentInstrumentId", new { OrganizationId = organizationId, PaymentInstrumentId = paymentInstrumentId }).SingleOrDefault();
                connection.Close();

                return customerLoyaltyPoint;
            }
        }

        private bool VerifySufficientLoyaltyPoints(int organizationId, long customerId, long paymentInstrumentId, long pointsToRedeem, out long inAccountPointBalance)
        {
            inAccountPointBalance = 0;
            CustomerLoyaltyPoint customerLoyaltyPoint = GetCustomerLoyaltyPointsByPaymentInstrument(organizationId, customerId, paymentInstrumentId);

            if (customerLoyaltyPoint == null)
                return false;

            if (pointsToRedeem > customerLoyaltyPoint.CumulativePoints)
            {
                inAccountPointBalance = customerLoyaltyPoint.CumulativePoints;
                return false;
            }
            else
            {
                inAccountPointBalance = customerLoyaltyPoint.CumulativePoints;
                return true;
            }
        }

        public CustomerLoyaltyPoint CreatePoints(int organizationId, long paymentInstrumentId, long amount)
        {
            try
            {
                double percentage = 0.02;

                double points1 = amount * percentage;

                long points = long.Parse(points1.ToString());
                CustomerLoyaltyPoint customerLoyaltyPoint = GetCustomerLoyaltyPointByMinified(organizationId, paymentInstrumentId);

                if (customerLoyaltyPoint != null)
                {
                    using (var connection = new SqlConnection(sqlConnectionString))
                    {
                        connection.Open();
                        var affectedRows = connection.Execute("UPDATE CustomerLoyaltyPoint SET CumulativePoints=@CumulativePoints, CumulativeTransactionAmount=@CumulativeTransactionAmount WHERE CustomerLoyaltyPointId = @CustomerLoyaltyPointId", new { CumulativePoints = customerLoyaltyPoint.CumulativePoints + points, CumulativeTransactionAmount = customerLoyaltyPoint.CumulativeTransactionAmount + amount, CustomerLoyaltyPointId = customerLoyaltyPoint.CustomerLoyaltyPointId });

                        connection.Close();

                        return GetCustomerLoyaltyPointByMinified(organizationId, paymentInstrumentId);
                    }

                }
                else
                {
                    long cumulativeFeeAmount = 0;
                    using (var connection = new SqlConnection(sqlConnectionString))
                    {
                        connection.Open();
                        var affectedRows = connection.Execute("INSERT INTO CustomerLoyaltyPoint (PaymentInstrumentId, CumulativeFeeAmount, CumulativePoints,CumulativeTransactionAmount,OrganizationId) VALUES (@PaymentInstrumentId, @CumulativeFeeAmount, @CumulativePoints,@CumulativeTransactionAmount,@OrganizationId)", new { paymentInstrumentId, cumulativeFeeAmount, points, amount, organizationId });

                        connection.Close();

                        return GetCustomerLoyaltyPointByMinified(organizationId, paymentInstrumentId);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        private long CalculateRedeemedLoyaltyAmount(long organizationId, long paymentInstrumentTypeId, long pointsToRedeem, out long redemptionRateId)
        {
            redemptionRateId = 0;
            LoyaltyRedemptionRate redemptionRate = null;

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                redemptionRate = connection.Query<LoyaltyRedemptionRate>("SELECT * FROM LoyaltyRedemptionRate WHERE OrganizationId=@OrganizationId AND PaymentInstrumentTypeId=@PaymentInstrumentTypeId", new { OrganizationId = organizationId, PaymentInstrumentTypeId = paymentInstrumentTypeId }).SingleOrDefault();
                connection.Close();

            }

            decimal redeemedAmount = 0;
            if (redemptionRate == null)
            {
                throw new Exception(string.Format("LY0006 - An active RedemptionRate for OrganizationId '{0}' and PaymentInstrumentTypeId '{1}' is not set.", organizationId, paymentInstrumentTypeId));
            }

            if (redemptionRate.RedemptionPercentageEquivalent == null)
            {
                redeemedAmount = pointsToRedeem * (decimal)redemptionRate.RedemptionAmountEquivalent / (decimal)redemptionRate.PointFrequency * baseCurrencyConversionUnit;
            }
            else
            {
                redeemedAmount = (decimal)redemptionRate.RedemptionPercentageEquivalent * pointsToRedeem * baseCurrencyConversionUnit;
            }

            redemptionRateId = redemptionRate.LoyaltyRedemptionRateId;
            return Convert.ToInt64(redeemedAmount);
        }


        public MasterTransactionRecord RedeemLoyaltyPoints(int organizationId, long customerId, long paymentInstrumentId, long pointsToRedeem)
        {
            HelperRepository helperRepository = new HelperRepository();
            TransactionType transactionType = helperRepository.GetTransactionTypeByTransactionTypeId((int)TransactionTypes.LoyaltyPointRedemption);

            if (!transactionType.IsActive)
                return null;

            long inAccountLoyaltyPoints = 0;
            if (!VerifySufficientLoyaltyPoints(organizationId, customerId, paymentInstrumentId, pointsToRedeem, out inAccountLoyaltyPoints))
            {
                throw new Exception(string.Format("LY0005 - The PaymentInstrumentId '{0}' has Insufficient Loyalty Points to perform this transaction under OrganizationId '{1}'.", paymentInstrumentId, organizationId));
            }

            //  ValidatePointRedemption(paymentInstrumentId, inAccountLoyaltyPoints, pointsToRedeem);

            BillPaymentRepository billRepository = new BillPaymentRepository();
            CustomerRepository customerRepository = new CustomerRepository();

            string loyaltyBillerNumber = "111111";
            Biller biller = billRepository.GetBillerByBillerNumber(loyaltyBillerNumber);

            if (biller == null)
            {
                throw new Exception("BL0019 - The LoyaltyTransactionsBiller is not set.");
            }

            Biller redemptionBiller = billRepository.GetBillerByBillerId(biller.BillerId);
            WalletTransactionsRepository walletTransactionsRepository = new WalletTransactionsRepository();

            string transactionNarrative = string.Empty;
            Customer redeemingCustomer = null;
            PaymentInstrument redeemingPaymentInstrument = null;

            DateTime transactionTime = GetRealDate();
            string transactionReference = HelperRepository.GenerateTransactionReferenceNumber();
            walletTransactionsRepository.MatchPaymentInstrumentToCustomer(customerId, paymentInstrumentId, out redeemingCustomer, out redeemingPaymentInstrument);

            long redemptionRateId = 0;
            long redeemedAmount = CalculateRedeemedLoyaltyAmount(organizationId, redeemingPaymentInstrument.PaymentInstrumentTypeId, pointsToRedeem, out redemptionRateId);

            if (redeemedAmount <= 0)
                return null;

            decimal senderBalanceBeforeTransaction = walletTransactionsRepository.BalanceEnquiry(redemptionBiller.CustomerId, redemptionBiller.OperationPaymentInstrumentId, string.Empty, false);
            decimal receiverBalanceBeforeTransaction = walletTransactionsRepository.BalanceEnquiry(redeemingCustomer.CustomerId, redeemingPaymentInstrument.PaymentInstrumentId, string.Empty, false);

            HelperRepository.CustomerHasSufficientFundsForTransaction(senderBalanceBeforeTransaction, redeemedAmount, 0);

            #region Picking Correct Transaction Narrative
            Biller redeemingBiller = null;
            switch (redeemingPaymentInstrument.PaymentInstrumentTypeId)
            {
                case (int)PITypes.MPESA:
                    long customerMsisdn = customerRepository.GetCustomerByCustomerId(customerId).Msisdn;
                    transactionNarrative = string.Format("From : {0} ({1}) - To : {2} {3} ({4})", redemptionBiller.BillerName, redemptionBiller.BillerNumber, redeemingCustomer.FirstName, redeemingCustomer.LastName, customerMsisdn);
                    break;
                case (int)PITypes.Voucher:
                    //AgentOutlet redeemingAgent = new AgentRepository().GetAgentOutletByTillCustomer(customerId);
                    //transactionNarrative = string.Format("From : {0} ({1}) - To : {2} ({3})", redemptionBiller.BillerName, redemptionBiller.BillerNumber, redeemingAgent.OutletName, redeemingAgent.OutletNumber);
                    break;
                case (int)PITypes.LoyaltySVA:
                    redeemingBiller = billRepository.GetBillerByCustomerId(customerId);
                    if (redeemingBiller == null)
                    {
                        throw new Exception(string.Format("BL0011 - The specified CustomerId '{0}' does not belong to any Biller", customerId));
                    }
                    redeemingBiller = billRepository.GetBillerByBillerId(redeemingBiller.BillerId);

                    transactionNarrative = string.Format("From : {0} ({1}) - To : {2} ({3})", redemptionBiller.BillerName, redemptionBiller.BillerNumber, redeemingBiller.BillerName, redeemingBiller.BillerNumber);
                    break;
                default:
                    throw new Exception(string.Format("PI0019 - The PaymentInstrumentId '{0}' is not allowed to perform this transaction.", redeemingPaymentInstrument.PaymentInstrumentId));
            }
            #endregion

            MasterTransactionRecord newRedemptionTransaction = new MasterTransactionRecord
            {
                Amount = redeemedAmount,
                Fee = 0,
                IsTestTransaction = redeemingCustomer.IsTestCustomer,
                ExternalApplicationId = 1,
                AccessChannelId = (int)AccessChannels.Android,
                ShortDescription = transactionType.FriendlyName,
                PayerId = redemptionBiller.CustomerId,
                PayeeId = redeemingCustomer.CustomerId,
                PayerPaymentInstrumentId = redemptionBiller.OperationPaymentInstrumentId,
                PayeePaymentInstrumentId = redeemingPaymentInstrument.PaymentInstrumentId,
                Tax = 0,
                Text = transactionNarrative,
                Text4 = redemptionRateId.ToString(),
                TransactionErrorCodeId = 1,
                TransactionDate = transactionTime,
                TransactionTypeId = transactionType.TransactionTypeId,
                TransactionReference = transactionReference,
                CustomerTypeId = redeemingCustomer.CustomerTypeId,
                // SourceUserName = redemptionBiller.Customer.UserName,
                DestinationUserName = redeemingCustomer.UserName,
                PayerBalanceBeforeTransaction = long.Parse(senderBalanceBeforeTransaction.ToString()),
                PayeeBalanceBeforeTransaction = long.Parse(receiverBalanceBeforeTransaction.ToString()),
                TransactionStatusId = TransactionState.Successful
            };


            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("INSERT INTO MasterTransactionRecord (Amount, Fee, IsTestTransaction,ExternalApplicationId,AccessChannelId,ShortDescription,PayerId,PayeeId,PayerPaymentInstrumentId,PayeePaymentInstrumentId,Tax,Text,Text4,TransactionErrorCodeId,TransactionDate,TransactionTypeId,TransactionReference,CustomerTypeId,DestinationUserName,PayerBalanceBeforeTransaction,PayeeBalanceBeforeTransaction,TransactionStatusId) VALUES (@Amount, @Fee, @IsTestTransaction,@ExternalApplicationId,@AccessChannelId,@ShortDescription,@PayerId,@PayeeId,@PayerPaymentInstrumentId,@PayeePaymentInstrumentId,@Tax,@Text,@Text4,@TransactionErrorCodeId,@TransactionDate,@TransactionTypeId@TransactionReference,@CustomerTypeId,@DestinationUserName,@PayerBalanceBeforeTransaction,@PayeeBalanceBeforeTransaction,@TransactionStatusId)", new { newRedemptionTransaction.Amount, newRedemptionTransaction.Fee, newRedemptionTransaction.IsTestTransaction, newRedemptionTransaction.ExternalApplicationId, newRedemptionTransaction.AccessChannelId, newRedemptionTransaction.ShortDescription, newRedemptionTransaction.PayerId, newRedemptionTransaction.PayeeId, newRedemptionTransaction.PayerPaymentInstrumentId, newRedemptionTransaction.PayeePaymentInstrumentId, newRedemptionTransaction.Tax, newRedemptionTransaction.Text, newRedemptionTransaction.Text4, newRedemptionTransaction.TransactionErrorCodeId, newRedemptionTransaction.TransactionDate, newRedemptionTransaction.TransactionTypeId, newRedemptionTransaction.TransactionReference, newRedemptionTransaction.CustomerTypeId, newRedemptionTransaction.DestinationUserName, newRedemptionTransaction.PayerBalanceBeforeTransaction, newRedemptionTransaction.PayeeBalanceBeforeTransaction, newRedemptionTransaction.TransactionStatusId });

                connection.Close();
            }

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                newRedemptionTransaction = connection.Query<MasterTransactionRecord>("SELECT * FROM MasterTransactionRecord WHERE TransactionDate=@TransactionDate AND PaymentInstrumentId=@PaymentInstrumentId AND PayeeId=@PayeeId", new { TransactionDate = newRedemptionTransaction.TransactionDate, PayeeId = newRedemptionTransaction.PayeeId }).SingleOrDefault();

                connection.Close();
            }

            #region Updating Balances

            if (redeemingPaymentInstrument.PaymentInstrumentTypeId == (int)PITypes.MPESA)
            {
                HelperRepository.UpdateCustomerSVABalance(redeemingPaymentInstrument, redeemedAmount, 0, true);
            }
            else
            {
                HelperRepository.UpdateSVABalance(redeemingPaymentInstrument, redeemedAmount, true);
            }

            HelperRepository.UpdateBillerSVABalance(redemptionBiller, redeemedAmount, transactionReference, false, string.Format("{0} - {1}", "HOOK", transactionType.FriendlyName));

            #endregion
            #region Sender GL

            BillerTransactionGL senderBillerTransactionGl = new BillerTransactionGL
            {
                BillerId = redemptionBiller.BillerId,
                MasterTransactionRecordId = newRedemptionTransaction.MasterTransactionRecordId,
                PaymentInstrumentId = redemptionBiller.OperationPaymentInstrumentId,
                TransactionDate = transactionTime,
                Amount = redeemedAmount,
                CommssionEarned = 0
            };

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("INSERT INTO BillerTransactionGL (BillerId, MasterTransactionRecordId, PaymentInstrumentId,TransactionDate,Amount,CommssionEarned) VALUES (@BillerId,@MasterTransactionRecordId,@PaymentInstrumentId,@TransactionDate,@Amount,@CommssionEarned)", new { senderBillerTransactionGl.BillerId, senderBillerTransactionGl.MasterTransactionRecordId, senderBillerTransactionGl.PaymentInstrumentId, senderBillerTransactionGl.TransactionDate, senderBillerTransactionGl.Amount, senderBillerTransactionGl.CommssionEarned });

                connection.Close();
            }
            #endregion

            #region Receiver GLs

            if (redeemingPaymentInstrument.PaymentInstrumentTypeId == (int)PITypes.MPESA)
            {
                CustomerTransactionGL recipientConsumerTransactionGl = new CustomerTransactionGL()
                {
                    CustomerId = redeemingCustomer.CustomerId,
                    MasterTransactionRecordId = newRedemptionTransaction.MasterTransactionRecordId,
                    TransactionDate = transactionTime
                };

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    var affectedRows = connection.Execute("INSERT INTO CustomerTransactionGL (CustomerId, MasterTransactionRecordId, TransactionDate) VALUES (@CustomerId,@MasterTransactionRecordId,@TransactionDate)", new { recipientConsumerTransactionGl.CustomerId, recipientConsumerTransactionGl.MasterTransactionRecordId, recipientConsumerTransactionGl.TransactionDate });

                    connection.Close();
                }
            }
            else if (redeemingPaymentInstrument.PaymentInstrumentTypeId == (int)PITypes.Voucher)
            {
                //AgentTransactionGL recipientAgentTransactionGl = new AgentTransactionGL
                //{
                //    TillCustomerId = redeemingCustomer.CustomerId,
                //    MasterTransactionRecordId = newRedemptionTransaction.MasterTransactionRecordId,
                //    TransactionDate = transactionTime,
                //    CommssionEarned = 0
                //};
                //context.AgentTransactionGLs.Add(recipientAgentTransactionGl);
            }
            else if (redeemingPaymentInstrument.PaymentInstrumentTypeId == (int)PITypes.LoyaltySVA)
            {
                //BillerTransactionGL recipientBillerTransactionGl = new BillerTransactionGL
                //{
                //    BillerId = redeemingBiller.BillerId,
                //    MasterTransactionRecordId = newRedemptionTransaction.MasterTransactionRecordId,
                //    PaymentInstrumentId = redeemingBiller.OperationPaymentInstrumentId,
                //    TransactionDate = transactionTime,
                //    Amount = redeemedAmount,
                //    CommssionEarned = 0
                //};
                //context.BillerTransactionGLs.Add(recipientBillerTransactionGl);
            }

            #endregion

            //context.FeesGLs.Add(feeGl);
            // context.SaveChanges();

            walletTransactionsRepository = new WalletTransactionsRepository();

            decimal senderBalanceAfterTransaction = walletTransactionsRepository.BalanceEnquiry(redemptionBiller.CustomerId, redemptionBiller.OperationPaymentInstrumentId, string.Empty, false);
            decimal receiverBalanceAfterTransaction = walletTransactionsRepository.BalanceEnquiry(customerId, paymentInstrumentId, string.Empty, false);

            newRedemptionTransaction.PayerBalanceAfterTransaction = long.Parse(senderBalanceAfterTransaction.ToString());
            newRedemptionTransaction.PayeeBalanceAfterTransaction = long.Parse(receiverBalanceAfterTransaction.ToString());

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("UPDATE MasterTransactionRecord SET PayerBalanceAfterTransaction=@PayerBalanceAfterTransaction, PayeeBalanceAfterTransaction=@PayeeBalanceAfterTransaction WHERE MasterTransactionRecordId = @MasterTransactionRecordId", new { PayerBalanceAfterTransaction = newRedemptionTransaction.PayerBalanceAfterTransaction, PayeeBalanceAfterTransaction = newRedemptionTransaction.PayeeBalanceAfterTransaction, MasterTransactionRecordId = newRedemptionTransaction.MasterTransactionRecordId });

                connection.Close();
            }

            //update the account before the money transaction and also the bot loyalty
            //long redeemerPointBalance = 0;
            //LoyaltyPointExecutionBot isSuccessfulTransfer = TransferLoyaltyPoints(organizationId, customerId, paymentInstrumentId, redemptionBiller.CustomerId, redemptionBiller.OperationPaymentInstrumentId, pointsToRedeem, newRedemptionTransaction, out redeemerPointBalance);

            LoyaltyPointExecutionBot isSuccessfulTransfer = TransferingLoyaltyPoints(organizationId, customerId, paymentInstrumentId, redemptionBiller.CustomerId, redemptionBiller.OperationPaymentInstrumentId, pointsToRedeem, newRedemptionTransaction);

            if (isSuccessfulTransfer == null)
            {
                string username = "system";
                walletTransactionsRepository.ReverseWalletTransaction(newRedemptionTransaction.MasterTransactionRecordId, username);
            }

            //sent SMS
            // messageRepository.LoyaltyPointRedemptionSms(redeemingCustomer, newRedemptionTransaction.TransactionReference, newRedemptionTransaction.Amount, pointsToRedeem, transactionTime, Convert.ToInt64(receiverBalanceAfterTransaction), redeemerPointBalance, newRedemptionTransaction.MasterTransactionRecordId);

            return newRedemptionTransaction;
        }

        public LoyaltyPointExecutionBot TransferingLoyaltyPoints(int organizationId, long sourceCustomerId, long sourcePaymentInstrumentId, long destinationCustomerId, long destinationPaymentInstrumentId, long pointsToTransfer, MasterTransactionRecord masterTransactionRecord)
        {
            TransactionRepository transactionRepository = new TransactionRepository();
            masterTransactionRecord = null;
            HelperRepository helperRespository = new HelperRepository();
            TransactionType transactionType = masterTransactionRecord == null ? helperRespository.GetTransactionTypeByTransactionTypeId((long)TransactionTypes.LoyaltyPointTransfer) : helperRespository.GetTransactionTypeByTransactionTypeId(masterTransactionRecord.TransactionTypeId);

            if (!transactionType.IsActive)
                return null;

            Customer senderCustomer = null;
            Customer recipientCustomer = null;
            PaymentInstrument senderPaymentInstrument = null;
            PaymentInstrument recipientPaymentInstrument = null;

            WalletTransactionsRepository walletTransactionsRepository = new WalletTransactionsRepository();


            walletTransactionsRepository.MatchPaymentInstrumentToCustomer(sourceCustomerId, sourcePaymentInstrumentId, out senderCustomer, out senderPaymentInstrument);
            walletTransactionsRepository.MatchPaymentInstrumentToCustomer(destinationCustomerId, destinationPaymentInstrumentId, out recipientCustomer, out recipientPaymentInstrument);

            #region Validation
            if (senderCustomer == null)
                throw new Exception(string.Format("CA0004 - CustomerId '{0}' does not exist", sourceCustomerId));

            if (recipientCustomer == null)
                throw new Exception(string.Format("CA0004 - CustomerId '{0}' does not exist", destinationCustomerId));

            if (senderPaymentInstrument == null)
                throw new Exception(string.Format("PI0001 - Payment Instrument '{0}' doesn’t exist", sourcePaymentInstrumentId));

            if (recipientPaymentInstrument == null)
                throw new Exception(string.Format("PI0001 - Payment Instrument '{0}' doesn’t exist", destinationPaymentInstrumentId));
            #endregion

            CustomerLoyaltyPoint sourceCustomerLoyaltyPoint = GetCustomerLoyaltyPointsByPaymentInstrument(organizationId, sourceCustomerId, sourcePaymentInstrumentId);

            if (sourceCustomerLoyaltyPoint == null)
            {
                throw new Exception(string.Format("LY0007 - CustomerId '{0}' does not have accrued loyalty  points for OrganizationId '{1}' and PaymentInstrumentId '{2}'.", sourceCustomerId, organizationId, sourcePaymentInstrumentId));
            }

            //if (masterTransactionRecord == null)
            //{
            //    ValidatePointRedemption(sourcePaymentInstrumentId, sourceCustomerLoyaltyPoint.CumulativePoints, pointsToTransfer);
            //}

            CustomerLoyaltyPoint destinationCustomerLoyaltyPoint = null;

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                destinationCustomerLoyaltyPoint = connection.Query<CustomerLoyaltyPoint>("SELECT * FROM CustomerLoyaltyPoint WHERE OrganizationId=@OrganizationId AND PaymentInstrumentId=@PaymentInstrumentId AND CustomerId=@CustomerId", new { OrganizationId = organizationId, PaymentInstrumentId = destinationPaymentInstrumentId, CustomerId = destinationCustomerId }).SingleOrDefault();
                connection.Close();
            }

            if (destinationCustomerLoyaltyPoint == null)
            {
                destinationCustomerLoyaltyPoint = new CustomerLoyaltyPoint
                {
                    OrganizationId = organizationId,
                    PaymentInstrumentId = destinationPaymentInstrumentId,
                    CumulativeFeeAmount = 0,
                    CumulativeTransactionAmount = 0,
                    CumulativePoints = 0
                };

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();

                    var affectedRows = connection.Execute("INSERT INTO CustomerLoyaltyPoint (OrganizationId, PaymentInstrumentId, CumulativeFeeAmount,CumulativeTransactionAmount,CumulativePoints) VALUES (@OrganizationId,@PaymentInstrumentId,@CumulativeFeeAmount,@CumulativeTransactionAmount,@CumulativePoints)", new { destinationCustomerLoyaltyPoint.OrganizationId, destinationCustomerLoyaltyPoint.PaymentInstrumentId, destinationCustomerLoyaltyPoint.CumulativeFeeAmount, destinationCustomerLoyaltyPoint.CumulativeTransactionAmount, destinationCustomerLoyaltyPoint.CumulativePoints });

                    connection.Close();
                }
            }

            long sourcePointBalanceAfterTransaction = sourceCustomerLoyaltyPoint.CumulativePoints -= pointsToTransfer;
            long destinationPointBalanceAfterTransaction = destinationCustomerLoyaltyPoint.CumulativePoints += pointsToTransfer;

            LoyaltyPointExecutionBot executionBot = new LoyaltyPointExecutionBot
            {
                OrganizationId = organizationId,
                TransactionTypeId = transactionType.TransactionTypeId,
                TransactionReference = masterTransactionRecord == null ? HelperRepository.GenerateTransactionReferenceNumber() : masterTransactionRecord.TransactionReference,
                TransactionFee = 0,
                TransactionAmount = pointsToTransfer,
                SourceCustomerId = sourceCustomerId,
                DestinationCustomerId = destinationCustomerId,
                SourcePaymentInstrumentId = sourcePaymentInstrumentId,
                DestinationPaymentInstrumentId = destinationPaymentInstrumentId,
                IsLoyaltyBasedOnAmountSchemeExecuted = true,
                IsLoyaltyBasedOnFrequencySchemeExecuted = true,
                IsLoyaltyBasedOnCumulativeAmountSchemeExecuted = true,
                DatePosted = GetRealDate(),
                MasterTransactionRecordId = masterTransactionRecord == null ? (long?)null : masterTransactionRecord.MasterTransactionRecordId,
                LoyaltyRedemptionRateId = masterTransactionRecord == null ? (int?)null : int.Parse(masterTransactionRecord.Text4),
                SourcePointBalanceBeforeTransaction = sourceCustomerLoyaltyPoint.CumulativePoints,
                DestinationPointBalanceBeforeTransaction = destinationCustomerLoyaltyPoint.CumulativePoints,
                SourcePointBalanceAfterTransaction = sourcePointBalanceAfterTransaction,
                DestinationPointBalanceAfterTransaction = destinationPointBalanceAfterTransaction
            };


            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("INSERT INTO LoyaltyPointExecutionBot (OrganizationId, TransactionTypeId, TransactionReference,TransactionFee,TransactionAmount,SourceCustomerId,DestinationCustomerId,SourcePaymentInstrumentId,DestinationPaymentInstrumentId,IsLoyaltyBasedOnAmountSchemeExecuted,IsLoyaltyBasedOnFrequencySchemeExecuted,IsLoyaltyBasedOnCumulativeAmountSchemeExecuted,DatePosted,MasterTransactionRecordId,LoyaltyRedemptionRateId,SourcePointBalanceBeforeTransaction,DestinationPointBalanceBeforeTransaction,SourcePointBalanceAfterTransaction,DestinationPointBalanceAfterTransaction) VALUES (@OrganizationId,@TransactionTypeId,@TransactionReference,@TransactionFee,@TransactionAmount,@SourceCustomerId,@DestinationCustomerId,@SourcePaymentInstrumentId,@DestinationPaymentInstrumentId,@IsLoyaltyBasedOnAmountSchemeExecuted,@IsLoyaltyBasedOnFrequencySchemeExecuted,@IsLoyaltyBasedOnCumulativeAmountSchemeExecuted,@DatePosted,@MasterTransactionRecordId,@LoyaltyRedemptionRateId,@SourcePointBalanceBeforeTransaction,@DestinationPointBalanceBeforeTransaction,@SourcePointBalanceAfterTransaction,@DestinationPointBalanceAfterTransaction)", new { executionBot.OrganizationId, executionBot.TransactionTypeId, executionBot.TransactionReference, executionBot.TransactionFee, executionBot.TransactionAmount, executionBot.SourceCustomerId, executionBot.DestinationCustomerId, executionBot.SourcePaymentInstrumentId, executionBot.DestinationPaymentInstrumentId, executionBot.IsLoyaltyBasedOnAmountSchemeExecuted, executionBot.IsLoyaltyBasedOnFrequencySchemeExecuted, executionBot.IsLoyaltyBasedOnCumulativeAmountSchemeExecuted, executionBot.DatePosted, executionBot.MasterTransactionRecordId, executionBot.LoyaltyRedemptionRateId, executionBot.SourcePointBalanceBeforeTransaction, executionBot.DestinationPointBalanceBeforeTransaction, executionBot.SourcePointBalanceAfterTransaction, executionBot.DestinationPointBalanceAfterTransaction });

                connection.Close();
            }

            #region Updating Account Balances

            transactionRepository.UpdateLoyaltyPaymentInstrumentByPaymentInstrumentId(sourcePaymentInstrumentId, sourcePointBalanceAfterTransaction);
            transactionRepository.UpdateLoyaltyPaymentInstrumentByPaymentInstrumentId(destinationPaymentInstrumentId, destinationPointBalanceAfterTransaction);


            //destinationBalanceAfterTransfer = (long)executionBot.DestinationPointBalanceAfterTransaction;

            #endregion

            //if (masterTransactionRecord == null)
            //{
            //    messageRepository.LoyaltyPointTransferSms(executionBot, senderPaymentInstrument, recipientPaymentInstrument);
            //}

            return executionBot;
        }

        public ReverseTransactionResult ReverseTransferedLoyaltyPoints(long loyaltyPointExecutionBotId, string reversedByUsername)
        {
            HelperRepository helperRepository = new HelperRepository();
            TransactionType reversalTransactionType = helperRepository.GetTransactionTypeByTransactionTypeId((int)TransactionTypes.ReversalTransaction);

            if (!reversalTransactionType.IsActive)
                return ReverseTransactionResult.TransactionReversalDisabled;

            helperRepository.ValidateUsername(reversedByUsername, false);

            LoyaltyPointExecutionBot loyaltyExecutionBot = null;

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                loyaltyExecutionBot = connection.Query<LoyaltyPointExecutionBot>("SELECT * FROM LoyaltyPointExecutionBot WHERE LoyaltyPointExecutionBotId=@LoyaltyPointExecutionBotId", new { LoyaltyPointExecutionBotId = loyaltyPointExecutionBotId }).SingleOrDefault();
                connection.Close();
            }

            if (loyaltyExecutionBot == null)
            {
                throw new Exception(string.Format("LY0008 - The LoyaltyPointExecutionBotId '{0}' does not exist.", loyaltyPointExecutionBotId));
            }

            ReversibleTransaction reversibleTransaction = null;

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                reversibleTransaction = connection.Query<ReversibleTransaction>("SELECT * FROM ReversibleTransaction WHERE TransactionTypeId=@TransactionTypeId", new { TransactionTypeId = loyaltyExecutionBot.TransactionTypeId }).SingleOrDefault();
                connection.Close();
            }

            if (reversibleTransaction == null)
            {
                return ReverseTransactionResult.TransactionNotReversible;
            }

            int count = 0;

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                count = connection.Query<LoyaltyPointExecutionBot>("SELECT * FROM LoyaltyPointExecutionBot WHERE LoyaltyPointExecutionBotId=@LoyaltyPointExecutionBotId", new { LoyaltyPointExecutionBotId = loyaltyPointExecutionBotId }).Count();
                connection.Close();
            }

            if (count > 0)
            {
                return ReverseTransactionResult.TransactionCannotBeReversedTwice;
            }

            CustomerLoyaltyPoint sourceCustomerLoyaltyPoint = null;

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                sourceCustomerLoyaltyPoint = connection.Query<CustomerLoyaltyPoint>("SELECT * FROM CustomerLoyaltyPoint WHERE OrganizationId=@OrganizationId AND PaymentInstrumentId=@PaymentInstrumentId AND CustomerId=@CustomerId", new { OrganizationId = loyaltyExecutionBot.OrganizationId, PaymentInstrumentId = (long)loyaltyExecutionBot.DestinationPaymentInstrumentId, CustomerId = (long)loyaltyExecutionBot.DestinationCustomerId }).SingleOrDefault();
                connection.Close();
            }

            if (sourceCustomerLoyaltyPoint == null)
            {
                throw new Exception(string.Format("LY0007 - CustomerId '{0}' does not have accrued loyalty points for OrganizationId '{1}' and PaymentInstrumentId '{2}'.", loyaltyExecutionBot.DestinationCustomerId, loyaltyExecutionBot.OrganizationId, loyaltyExecutionBot.DestinationPaymentInstrumentId));
            }

            CustomerLoyaltyPoint destinationCustomerLoyaltyPoint = null;

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                destinationCustomerLoyaltyPoint = connection.Query<CustomerLoyaltyPoint>("SELECT * FROM CustomerLoyaltyPoint WHERE OrganizationId=@OrganizationId AND PaymentInstrumentId=@PaymentInstrumentId AND CustomerId=@CustomerId", new { OrganizationId = loyaltyExecutionBot.OrganizationId, PaymentInstrumentId = (long)loyaltyExecutionBot.SourcePaymentInstrumentId, CustomerId = (long)loyaltyExecutionBot.SourceCustomerId }).SingleOrDefault();
                connection.Close();
            }

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                sourceCustomerLoyaltyPoint = connection.Query<CustomerLoyaltyPoint>("SELECT * FROM CustomerLoyaltyPoint WHERE OrganizationId=@OrganizationId AND PaymentInstrumentId=@PaymentInstrumentId AND CustomerId=@CustomerId", new { OrganizationId = loyaltyExecutionBot.OrganizationId, PaymentInstrumentId = (long)loyaltyExecutionBot.DestinationPaymentInstrumentId, CustomerId = (long)loyaltyExecutionBot.DestinationCustomerId }).SingleOrDefault();
                connection.Close();
            }

            if (destinationCustomerLoyaltyPoint == null)
            {
                throw new Exception(string.Format("LY0007 - CustomerId '{0}' does not have accrued loyalty points for OrganizationId '{1}' and PaymentInstrumentId '{2}'.", loyaltyExecutionBot.SourceCustomerId, loyaltyExecutionBot.OrganizationId, loyaltyExecutionBot.SourcePaymentInstrumentId));
            }

            if (sourceCustomerLoyaltyPoint.CumulativePoints < loyaltyExecutionBot.TransactionAmount)
            {
                throw new Exception(string.Format("LY0005 - The PaymentInstrumentId '{0}' has Insufficient Loyalty Points to perform this transaction under OrganizationId '{1}'.", loyaltyExecutionBot.DestinationPaymentInstrumentId, loyaltyExecutionBot.OrganizationId));
            }

            long sourcePointBalanceAfterTransaction = sourceCustomerLoyaltyPoint.CumulativePoints -= loyaltyExecutionBot.TransactionAmount;
            long destinationPointBalanceAfterTransaction = destinationCustomerLoyaltyPoint.CumulativePoints += loyaltyExecutionBot.TransactionAmount;

            LoyaltyPointExecutionBot executionBot = new LoyaltyPointExecutionBot
            {
                OrganizationId = loyaltyExecutionBot.OrganizationId,
                TransactionTypeId = reversalTransactionType.TransactionTypeId,
                TransactionReference = loyaltyExecutionBot.TransactionReference + "(Rv)",
                TransactionFee = 0,
                TransactionAmount = loyaltyExecutionBot.TransactionAmount,
                SourceCustomerId = loyaltyExecutionBot.DestinationCustomerId,
                DestinationCustomerId = loyaltyExecutionBot.SourceCustomerId,
                SourcePaymentInstrumentId = loyaltyExecutionBot.DestinationPaymentInstrumentId,
                DestinationPaymentInstrumentId = loyaltyExecutionBot.SourcePaymentInstrumentId,
                IsLoyaltyBasedOnAmountSchemeExecuted = true,
                IsLoyaltyBasedOnFrequencySchemeExecuted = true,
                IsLoyaltyBasedOnCumulativeAmountSchemeExecuted = true,
                DatePosted = GetRealDate(),
                MasterTransactionRecordId = loyaltyExecutionBot.MasterTransactionRecordId,
                LoyaltyRedemptionRateId = loyaltyExecutionBot.LoyaltyRedemptionRateId,
                SourcePointBalanceBeforeTransaction = destinationCustomerLoyaltyPoint.CumulativePoints,
                DestinationPointBalanceBeforeTransaction = sourceCustomerLoyaltyPoint.CumulativePoints,
                ReversedTransactionParticulars = loyaltyExecutionBot.LoyaltyPointExecutionBotId.ToString(),
                SourcePointBalanceAfterTransaction = sourcePointBalanceAfterTransaction,
                DestinationPointBalanceAfterTransaction = destinationPointBalanceAfterTransaction
            };


            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("INSERT INTO LoyaltyPointExecutionBot (OrganizationId, TransactionTypeId, TransactionReference,TransactionFee,TransactionAmount,SourceCustomerId,DestinationCustomerId,SourcePaymentInstrumentId,DestinationPaymentInstrumentId,IsLoyaltyBasedOnAmountSchemeExecuted,IsLoyaltyBasedOnFrequencySchemeExecuted,IsLoyaltyBasedOnCumulativeAmountSchemeExecuted,DatePosted,MasterTransactionRecordId,LoyaltyRedemptionRateId,SourcePointBalanceBeforeTransaction,DestinationPointBalanceBeforeTransaction,SourcePointBalanceAfterTransaction,DestinationPointBalanceAfterTransaction) VALUES (@OrganizationId,@TransactionTypeId,@TransactionReference,@TransactionFee,@TransactionAmount,@SourceCustomerId,@DestinationCustomerId,@SourcePaymentInstrumentId,@DestinationPaymentInstrumentId,@IsLoyaltyBasedOnAmountSchemeExecuted,@IsLoyaltyBasedOnFrequencySchemeExecuted,@IsLoyaltyBasedOnCumulativeAmountSchemeExecuted,@DatePosted,@MasterTransactionRecordId,@LoyaltyRedemptionRateId,@SourcePointBalanceBeforeTransaction,@DestinationPointBalanceBeforeTransaction,@SourcePointBalanceAfterTransaction,@DestinationPointBalanceAfterTransaction)", new { executionBot.OrganizationId, executionBot.TransactionTypeId, executionBot.TransactionReference, executionBot.TransactionFee, executionBot.TransactionAmount, executionBot.SourceCustomerId, executionBot.DestinationCustomerId, executionBot.SourcePaymentInstrumentId, executionBot.DestinationPaymentInstrumentId, executionBot.IsLoyaltyBasedOnAmountSchemeExecuted, executionBot.IsLoyaltyBasedOnFrequencySchemeExecuted, executionBot.IsLoyaltyBasedOnCumulativeAmountSchemeExecuted, executionBot.DatePosted, executionBot.MasterTransactionRecordId, executionBot.LoyaltyRedemptionRateId, executionBot.SourcePointBalanceBeforeTransaction, executionBot.DestinationPointBalanceBeforeTransaction, executionBot.SourcePointBalanceAfterTransaction, executionBot.DestinationPointBalanceAfterTransaction });

                connection.Close();
            }

            #region Updating Account Balances
            //sourceCustomerLoyaltyPoint.CumulativePoints -= loyaltyExecutionBot.TransactionAmount;
            ////sourceCustomerLoyaltyPoint.TransactionFrequency -= initialTransactionFrequency;

            //destinationCustomerLoyaltyPoint.CumulativePoints += loyaltyExecutionBot.TransactionAmount;
            ////destinationCustomerLoyaltyPoint.TransactionFrequency -= initialTransactionFrequency;

            //executionBot.SourcePointBalanceAfterTransaction = destinationCustomerLoyaltyPoint.CumulativePoints;
            //executionBot.DestinationPointBalanceAfterTransaction = sourceCustomerLoyaltyPoint.CumulativePoints;

            //context.SaveChanges();
            #endregion

            //    messageRepository.LoyaltyPointReversalSms(executionBot);

            return ReverseTransactionResult.TransactionBookedForReversal;
        }

        public DateTime GetRealDate()
        {
            //Set the time zone information to E. Africa Standard Time 
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time");
            //Get date and time in US Mountain Standard Time 
            DateTime dateTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);

            return dateTime;
        }

        //public LoyaltyRedemptionRate CreateLoyaltyRedemptionRate(int organizationId, int paymentInstrumentTypeId, long redemptionPointFrequency, long redemptionAmountEquivalent, decimal redemptionPercentage, string createdByUsername)
        //{
        //    try
        //    {
        //        HelperRepository helperRepository = new HelperRepository();
        //        LoyaltyRedemptionRate redemptionRate = null;

        //        helperRepository.ValidateUsername(createdByUsername, false);

        //        redemptionRate = context.LoyaltyRedemptionRates.FirstOrDefault(rate => rate.OrganizationId == organizationId && rate.PaymentInstrumentTypeId == paymentInstrumentTypeId && rate.IsActive == true);

        //        if (redemptionRate != null)
        //        {
        //            throw new Exception(string.Format("LY0001 - An active LoyaltyRedemptionRate for PaymentInstrumentTypeId '{0}' and OrganizationId '{1}' exists.", paymentInstrumentTypeId, organizationId));
        //        }

        //        Organization organization = context.Organizations.Find(organizationId);

        //        if (organization == null)
        //        {
        //            throw new Exception(string.Format("GN0008 - The specified OrganizationId '{0}' does not exist.", organizationId));
        //        }
        //        if (!organization.IsActive)
        //        {
        //            throw new Exception(string.Format("GN0009 - The OrganizationId '{0}' cannot be used because it is inactive.", organizationId));
        //        }

        //        PaymentInstrumentType paymentInstrumentType = helperRepository.PaymentInstrumentTypeExists(paymentInstrumentTypeId);

        //        redemptionRate = new LoyaltyRedemptionRate
        //        {
        //            OrganizationId = organization.OrganizationId,
        //            PaymentInstrumentTypeId = paymentInstrumentType.PaymentInstrumentTypeId,
        //            PointFrequency = redemptionPointFrequency == 0 ? (long?)null : redemptionPointFrequency,
        //            RedemptionAmountEquivalent = redemptionAmountEquivalent == 0 ? null : (long?)redemptionAmountEquivalent,
        //            RedemptionPercentageEquivalent = redemptionPercentage == 0 ? null as decimal? : redemptionPercentage,
        //            IsActive = true,
        //            CreatedDate = DateTime.Now,
        //            CreatedByUsername = createdByUsername.ToLower()
        //        };
        //        context.LoyaltyRedemptionRates.Add(redemptionRate);
        //        context.SaveChanges();

        //        return Mapper.Map<LoyaltyRedemptionRate, LoyaltyRedemptionRateDto>(redemptionRate);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //public LoyaltyRedemptionRate EnableOrDisableLoyaltyRedemptionRate(int loyaltyRedemptionRateId, bool isActive, string updatedByUsername)
        //{
        //    try
        //    {
        //        HelperRepository helperRepository = new HelperRepository();
        //        helperRepository.ValidateUsername(updatedByUsername, false);

        //        LoyaltyRedemptionRate redemptionRate = context.LoyaltyRedemptionRates.Find(loyaltyRedemptionRateId);

        //        if (redemptionRate == null)
        //        {
        //            throw new Exception(string.Format("LY0002 - The specified LoyaltyRedemptionRateId '{0}' does not exist.", loyaltyRedemptionRateId));
        //        }

        //        redemptionRate.IsActive = isActive;
        //        redemptionRate.UpdatedDate = DateTime.Now;
        //        redemptionRate.UpdatedByUsername = updatedByUsername.ToLower();

        //        context.SaveChanges();

        //        return Mapper.Map<LoyaltyRedemptionRate, LoyaltyRedemptionRateDto>(redemptionRate);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public LoyaltyRedemptionRate UpdateLoyaltyRedemptionRate(int loyaltyRedemptionRateId, long redemptionPointFrequency, long redemptionAmountEquivalent, decimal redemptionPercentage, string updatedByUsername)
        //{
        //    try
        //    {
        //        HelperRepository helperRepository = new HelperRepository();
        //        helperRepository.ValidateUsername(updatedByUsername, false);

        //        LoyaltyRedemptionRate redemptionRateToUpdate = context.LoyaltyRedemptionRates.Find(loyaltyRedemptionRateId);

        //        if (redemptionRateToUpdate == null)
        //        {
        //            throw new Exception(string.Format("LY0002 - The specified LoyaltyRedemptionRateId '{0}' does not exist.", loyaltyRedemptionRateId));
        //        }

        //        LoyaltyPointExecutionBot executionBot = context.LoyaltyPointExecutionBots.FirstOrDefault(bot => bot.LoyaltyRedemptionRateId == loyaltyRedemptionRateId);
        //        if (executionBot != null)
        //        {
        //            throw new Exception(string.Format("LY0003 - LoyaltyRedemptionRateId '{0}' is used in other transactions thus cannot be updated.", loyaltyRedemptionRateId));
        //        }

        //        redemptionRateToUpdate.PointFrequency = redemptionPointFrequency == 0 ? (long?)null : redemptionPointFrequency;
        //        redemptionRateToUpdate.RedemptionAmountEquivalent = redemptionAmountEquivalent == 0 ? (long?)null : redemptionAmountEquivalent;
        //        redemptionRateToUpdate.RedemptionPercentageEquivalent = redemptionPercentage == 0 ? (decimal?)null : redemptionPercentage;
        //        redemptionRateToUpdate.UpdatedDate = DateTime.Now;
        //        redemptionRateToUpdate.UpdatedByUsername = updatedByUsername.ToLower();

        //        context.SaveChanges();

        //        return Mapper.Map<LoyaltyRedemptionRate, LoyaltyRedemptionRateDto>(redemptionRateToUpdate);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //public LoyaltyRedemptionRate GetLoyaltyRedemptionRateByRedemptionRateId(int loyaltyRedemptionRateId)
        //{
        //    try
        //    {
        //        LoyaltyRedemptionRate redemptionRate = context.LoyaltyRedemptionRates.Find(loyaltyRedemptionRateId);

        //        return Mapper.Map<LoyaltyRedemptionRate, LoyaltyRedemptionRateDto>(redemptionRate);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public LoyaltyRedemptionRate GetloyaltyRedemptionRateByPaymentInstrumentTypeId(int organizationId, int paymentInstrumentTypeId)
        //{
        //    try
        //    {
        //        LoyaltyRedemptionRate redemptionRate = context.LoyaltyRedemptionRates.SingleOrDefault(rate => rate.OrganizationId == organizationId && rate.PaymentInstrumentTypeId == paymentInstrumentTypeId && rate.IsActive == true);
        //        return Mapper.Map<LoyaltyRedemptionRate, LoyaltyRedemptionRateDto>(redemptionRate);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public LoyaltyTransactionFrequencySchemeDto GetLoyaltyTransactionFrequencyScheme(int organizationId, int transactionTypeId, int paymentInstrumentTypeId)
        //{
        //    try
        //    {
        //        LoyaltyTransactionFrequencyScheme loyaltyTransactionFrequencyBand = context.LoyaltyTransactionFrequencySchemes.SingleOrDefault(band => band.OrganizationId == organizationId && band.TransactionTypeId == transactionTypeId && band.PaymentInstrumentTypeId == paymentInstrumentTypeId);
        //        return Mapper.Map<LoyaltyTransactionFrequencyScheme, LoyaltyTransactionFrequencySchemeDto>(loyaltyTransactionFrequencyBand);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //public LoyaltyTransactionAmountSchemeDto GetLoyaltyTransactionAmountScheme(int organizationId, int transactionTypeId, int paymentInstrumentTypeId)
        //{
        //    try
        //    {
        //        LoyaltyTransactionAmountScheme loyaltyTransactionAmountBand = context.LoyaltyTransactionAmountSchemes.SingleOrDefault(band => band.OrganizationId == organizationId && band.TransactionTypeId == transactionTypeId && band.PaymentInstrumentTypeId == paymentInstrumentTypeId);
        //        return Mapper.Map<LoyaltyTransactionAmountScheme, LoyaltyTransactionAmountSchemeDto>(loyaltyTransactionAmountBand);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public LoyaltyCumulativeTransactionAmountSchemeDto GetLoyaltyCumulativeTransactionAmountScheme(int organizationId, int transactionTypeId, int paymentInstrumentTypeId)
        //{
        //    try
        //    {
        //        LoyaltyCumulativeTransactionAmountScheme cumulativeTransactionAmountScheme = context.LoyaltyCumulativeTransactionAmountSchemes.SingleOrDefault(band => band.OrganizationId == organizationId && band.TransactionTypeId == transactionTypeId && band.PaymentInstrumentTypeId == paymentInstrumentTypeId);
        //        return Mapper.Map<LoyaltyCumulativeTransactionAmountScheme, LoyaltyCumulativeTransactionAmountSchemeDto>(cumulativeTransactionAmountScheme);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public LoyaltyTransactionAmountSchemeItemDto GetLoyaltyTransactionAmountSchemeBand(long loyaltyTransactionAmountSchemeId, long amount)
        //{
        //    try
        //    {
        //        LoyaltyTransactionAmountSchemeItem schemeItem = context.LoyaltyTransactionAmountSchemeItems.SingleOrDefault(item => item.LoyaltyTransactionAmountSchemeId == loyaltyTransactionAmountSchemeId && item.MinimumValue <= amount && item.MaximumValue >= amount);
        //        return Mapper.Map<LoyaltyTransactionAmountSchemeItem, LoyaltyTransactionAmountSchemeItemDto>(schemeItem);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        ////}
        //public LoyaltyPointExecutionBot TransferLoyaltyPoints(int organizationId, long sourceCustomerId, long sourcePaymentInstrumentId, long destinationCustomerId, long destinationPaymentInstrumentId, long pointsToTransfer, MasterTransactionRecord masterTransactionRecord, out long destinationBalanceAfterTransfer)
        //{
        //    destinationBalanceAfterTransfer = 0;
        //    HelperRepository helperRespository = new HelperRepository();
        //    TransactionType transactionType = masterTransactionRecord == null ? helperRespository.GetTransactionTypeByTransactionTypeId((int)TransactionTypes.LoyaltyPointTransfer) : helperRespository.GetTransactionTypeByTransactionTypeId(masterTransactionRecord.TransactionTypeId);

        //    if (!transactionType.IsActive)
        //        return null;

        //    Customer senderCustomer = null;
        //    Customer recipientCustomer = null;
        //    PaymentInstrument senderPaymentInstrument = null;
        //    PaymentInstrument recipientPaymentInstrument = null;

        //    WalletTransactionsRepository walletTransactionsRepository = new WalletTransactionsRepository();


        //    walletTransactionsRepository.MatchPaymentInstrumentToCustomer(sourceCustomerId, sourcePaymentInstrumentId, out senderCustomer, out senderPaymentInstrument);
        //    walletTransactionsRepository.MatchPaymentInstrumentToCustomer(destinationCustomerId, destinationPaymentInstrumentId, out recipientCustomer, out recipientPaymentInstrument);

        //    #region Validation
        //    if (senderCustomer == null)
        //        throw new Exception(string.Format("CA0004 - CustomerId '{0}' does not exist", sourceCustomerId));

        //    if (recipientCustomer == null)
        //        throw new Exception(string.Format("CA0004 - CustomerId '{0}' does not exist", destinationCustomerId));

        //    if (senderPaymentInstrument == null)
        //        throw new Exception(string.Format("PI0001 - Payment Instrument '{0}' doesn’t exist", sourcePaymentInstrumentId));

        //    if (recipientPaymentInstrument == null)
        //        throw new Exception(string.Format("PI0001 - Payment Instrument '{0}' doesn’t exist", destinationPaymentInstrumentId));
        //    #endregion

        //    CustomerLoyaltyPoint sourceCustomerLoyaltyPoint = GetCustomerLoyaltyPointsByPaymentInstrument(organizationId, sourceCustomerId, sourcePaymentInstrumentId);

        //    if (sourceCustomerLoyaltyPoint == null)
        //    {
        //        throw new Exception(string.Format("LY0007 - CustomerId '{0}' does not have accrued loyalty  points for OrganizationId '{1}' and PaymentInstrumentId '{2}'.", sourceCustomerId, organizationId, sourcePaymentInstrumentId));
        //    }

        //    if (masterTransactionRecord == null)
        //    {
        //        ValidatePointRedemption(sourcePaymentInstrumentId, sourceCustomerLoyaltyPoint.CumulativePoints, pointsToTransfer);
        //    }

        //    CustomerLoyaltyPoint destinationCustomerLoyaltyPoint = context.CustomerLoyaltyPoints.SingleOrDefault(point => point.OrganizationId == organizationId && point.PaymentInstrumentId == destinationPaymentInstrumentId && point.PaymentInstrument.CustomerId == destinationCustomerId);

        //    if (destinationCustomerLoyaltyPoint == null)
        //    {
        //        destinationCustomerLoyaltyPoint = new CustomerLoyaltyPoint
        //        {
        //            OrganizationId = organizationId,
        //            PaymentInstrumentId = destinationPaymentInstrumentId,
        //            CumulativeFeeAmount = 0,
        //            CumulativeTransactionAmount = 0,
        //            CumulativePoints = 0
        //        };
        //        context.CustomerLoyaltyPoints.Add(destinationCustomerLoyaltyPoint);
        //    }


        //    LoyaltyPointExecutionBot executionBot = new LoyaltyPointExecutionBot
        //    {
        //        OrganizationId = organizationId,
        //        TransactionTypeId = transactionType.TransactionTypeId,
        //        TransactionReference = masterTransactionRecord == null ? HelperRepository.GenerateTransactionReferenceNumber() : masterTransactionRecord.TransactionReference,
        //        TransactionFee = 0,
        //        TransactionAmount = pointsToTransfer,
        //        SourceCustomerId = sourceCustomerId,
        //        DestinationCustomerId = destinationCustomerId,
        //        SourcePaymentInstrumentId = sourcePaymentInstrumentId,
        //        DestinationPaymentInstrumentId = destinationPaymentInstrumentId,
        //        IsLoyaltyBasedOnAmountSchemeExecuted = true,
        //        IsLoyaltyBasedOnFrequencySchemeExecuted = true,
        //        IsLoyaltyBasedOnCumulativeAmountSchemeExecuted = true,
        //        DatePosted = DateTime.Now,
        //        MasterTransactionRecordId = masterTransactionRecord == null ? (long?)null : masterTransactionRecord.MasterTransactionRecordId,
        //        LoyaltyRedemptionRateId = masterTransactionRecord == null ? (int?)null : int.Parse(masterTransactionRecord.Text4),
        //        SourcePointBalanceBeforeTransaction = sourceCustomerLoyaltyPoint.CumulativePoints,
        //        DestinationPointBalanceBeforeTransaction = destinationCustomerLoyaltyPoint.CumulativePoints
        //    };

        //    context.LoyaltyPointExecutionBots.Add(executionBot);
        //    context.SaveChanges();

        //    #region Updating Account Balances

        //    sourceCustomerLoyaltyPoint.CumulativePoints -= pointsToTransfer;
        //    destinationCustomerLoyaltyPoint.CumulativePoints += pointsToTransfer;
        //    executionBot.SourcePointBalanceAfterTransaction = sourceCustomerLoyaltyPoint.CumulativePoints;
        //    executionBot.DestinationPointBalanceAfterTransaction = destinationCustomerLoyaltyPoint.CumulativePoints;
        //    context.SaveChanges();


        //    senderPaymentInstrument = context.PaymentInstruments.Find(sourcePaymentInstrumentId);
        //    recipientPaymentInstrument = context.PaymentInstruments.Find(destinationPaymentInstrumentId);

        //    senderPaymentInstrument.LoyaltyPointBalance = sourceCustomerLoyaltyPoint.CumulativePoints;
        //    recipientPaymentInstrument.LoyaltyPointBalance = destinationCustomerLoyaltyPoint.CumulativePoints;
        //    context.SaveChanges();

        //    destinationBalanceAfterTransfer = (long)executionBot.DestinationPointBalanceAfterTransaction;

        //    #endregion

        //    if (masterTransactionRecord == null)
        //    {
        //        messageRepository.LoyaltyPointTransferSms(executionBot, senderPaymentInstrument, recipientPaymentInstrument);
        //    }

        //    return executionBot;
        //}


        //public List<int> GetSupportedLoyaltyTransactionTypeIds
        //{
        //    get
        //    {
        //        try
        //        {
        //            List<int> loyaltyFrequencyTransactionTypeIds = context.LoyaltyTransactionFrequencySchemes.Select(x => x.TransactionTypeId).ToList();
        //            List<int> loyaltyAmountTransactionTypeIds = context.LoyaltyTransactionAmountSchemes.Select(x => x.TransactionTypeId).ToList();
        //            List<int> loyaltyCumulativeAmountTransactionTypeIds = context.LoyaltyCumulativeTransactionAmountSchemes.Select(x => x.TransactionTypeId).ToList();

        //            List<int> minifiedUnion = loyaltyAmountTransactionTypeIds.Union(loyaltyCumulativeAmountTransactionTypeIds).ToList();

        //            List<int> unionOfloyaltyTransactionTypeIds = loyaltyFrequencyTransactionTypeIds.Union(minifiedUnion).ToList();

        //            return unionOfloyaltyTransactionTypeIds;
        //        }
        //        catch (Exception)
        //        {
        //            throw;
        //        }
        //    }
        //}

        //public bool ValidateTransactionTypeIdIsLoyaltySchemeSupported(int transactionTypeId)
        //{
        //    try
        //    {
        //        bool isTransactionTypeIdSupported = GetSupportedLoyaltyTransactionTypeIds.Intersect(new List<int> { transactionTypeId }).Count() == 0 ? false : true;
        //        return isTransactionTypeIdSupported;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //public string ProcessLoyaltyEarningRecord(LoyaltyPointExecutionBot processingLoyaltyRecord)
        //{
        //    try
        //    {
        //        //TODO: Save the record here and update it according to the scheme - why ?coz and using threads to do this and should be very independent
        //        context.LoyaltyPointExecutionBots.Add(processingLoyaltyRecord);
        //        context.SaveChanges();

        //        ProcessBotExecutionRecordByAmountScheme(processingLoyaltyRecord);
        //        ProcessBotExecutionRecordByFrequencyScheme(processingLoyaltyRecord);
        //        ProcessBotRecordByCumulativeTransactionAmountScheme(processingLoyaltyRecord);

        //        Parallel.Invoke(
        //          () =>
        //          {
        //              Task.Factory.StartNew(() => ProcessBotExecutionRecordByAmountScheme(processingLoyaltyRecord))
        //                .ContinueWith(tsk =>
        //                {
        //                    var flattened = tsk.Exception.Flatten();
        //                    flattened.Handle(ex =>
        //                    {
        //                        messageRepository.GenericExceptionEmail("info@iloyal.com", "Error: Processing Loyalty Points By Amount", string.Format("BOT EXECUTION RECORD BY AMOUNT-SCHEME EXCEPTION<br><br>{0}<br><br> Stack trace<br><br>{1}", ex.Message, ex.StackTrace), string.Empty);
        //                        return true;
        //                    });
        //                }, TaskContinuationOptions.OnlyOnFaulted);
        //          },
        //           () =>
        //           {
        //               Task.Factory.StartNew(() => ProcessBotExecutionRecordByFrequencyScheme(processingLoyaltyRecord))
        //                 .ContinueWith(tsk =>
        //                 {
        //                     var flattened = tsk.Exception.Flatten();
        //                     flattened.Handle(ex =>
        //                     {
        //                         messageRepository.GenericExceptionEmail("info@iloyal.com", "Error: Processing Loyalty Points By Frequency", string.Format("BOT EXECUTION RECORD BY FREQUENCY-SCHEME EXCEPTION<br><br>{0}<br><br> Stack trace<br><br>{1}", ex.Message, ex.StackTrace), string.Empty);
        //                         return true;
        //                     });
        //                 }, TaskContinuationOptions.OnlyOnFaulted);
        //           },
        //            () =>
        //            {
        //                Task.Factory.StartNew(() => ProcessBotRecordByCumulativeTransactionAmountScheme(processingLoyaltyRecord))
        //                  .ContinueWith(tsk =>
        //                  {
        //                      var flattened = tsk.Exception.Flatten();
        //                      flattened.Handle(ex =>
        //                      {
        //                          messageRepository.GenericExceptionEmail("info@iloyal.com", "Error: Processing Loyalty Points By Cumulative Amount", string.Format("BOT EXECUTION RECORD BY CUMULATIVE-AMOUNT-SCHEME EXCEPTION<br><br>{0}<br><br> Stack trace<br><br>{1}", ex.Message, ex.StackTrace), string.Empty);
        //                          return true;
        //                      });
        //                  }, TaskContinuationOptions.OnlyOnFaulted);
        //            });

        //        return "Success";

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //public string ProcessBotExecutionRecordByAmountScheme(LoyaltyPointExecutionBot botLoyaltyRecord)
        //{
        //    try
        //    {
        //        if (botLoyaltyRecord == null)
        //            return null;
        //        PaymentInstrument sourcePaymentInstrument = context.PaymentInstruments.Find(botLoyaltyRecord.SourcePaymentInstrumentId);
        //        if (sourcePaymentInstrument == null)
        //            return null;

        //        LoyaltyTransactionAmountSchemeDto loyaltyTransactionAmountScheme = GetLoyaltyTransactionAmountScheme(botLoyaltyRecord.OrganizationId, botLoyaltyRecord.TransactionTypeId, sourcePaymentInstrument.PaymentInstrumentTypeId);

        //        if (loyaltyTransactionAmountScheme == null)
        //            return null;

        //        if (!loyaltyTransactionAmountScheme.IsEligibleToAccruePoints)
        //            return null;

        //        LoyaltyTransactionAmountSchemeItemDto loyaltyTransactionAmountSchemeBand = GetLoyaltyTransactionAmountSchemeBand(loyaltyTransactionAmountScheme.LoyaltyTransactionAmountSchemeId,
        //                        botLoyaltyRecord.TransactionAmount);

        //        if (loyaltyTransactionAmountSchemeBand == null)
        //        {
        //            throw new Exception(string.Format("LY0004 - The LoyaltyTransactionAmountSchemeId '{0}' does not have limits set for amount '{1}'.", loyaltyTransactionAmountScheme.LoyaltyTransactionAmountSchemeId, botLoyaltyRecord.TransactionAmount));
        //        }

        //        List<LoyaltyPointExecutionBot> toProcessBotRecords = context.LoyaltyPointExecutionBots.Where(bot => bot.SourceCustomerId == botLoyaltyRecord.SourceCustomerId && bot.SourcePaymentInstrumentId == botLoyaltyRecord.SourcePaymentInstrumentId && bot.OrganizationId == botLoyaltyRecord.OrganizationId && bot.TransactionTypeId == botLoyaltyRecord.TransactionTypeId && bot.IsLoyaltyBasedOnAmountSchemeExecuted == false && bot.TransactionAmount >= loyaltyTransactionAmountSchemeBand.MinimumValue && bot.TransactionAmount <= loyaltyTransactionAmountSchemeBand.MaximumValue).ToList();

        //        int recordCount = toProcessBotRecords.Count;

        //        if (recordCount <= 0)
        //            return null;

        //        var loyaltyPointsEarned = toProcessBotRecords.GroupBy(p => new { p.OrganizationId, p.SourceCustomerId, p.SourcePaymentInstrumentId, p.TransactionTypeId })
        //                                    .Select(x => new
        //                                    {
        //                                        TotalFeeAmount = x.Sum(g => g.TransactionFee),
        //                                        TotalTransactionalAmount = x.Sum(g => g.TransactionAmount)
        //                                    });


        //        CustomerLoyaltyPoint customerLoyaltyPoint = context.CustomerLoyaltyPoints.SingleOrDefault(x => x.OrganizationId == botLoyaltyRecord.OrganizationId && x.PaymentInstrumentId == (long)botLoyaltyRecord.SourcePaymentInstrumentId);

        //        if (customerLoyaltyPoint != null)
        //        {
        //            //customerLoyaltyPoint.TransactionFrequency += initialTransactionFrequency;
        //            customerLoyaltyPoint.CumulativeFeeAmount += loyaltyPointsEarned.Single().TotalFeeAmount;
        //            customerLoyaltyPoint.CumulativeTransactionAmount += loyaltyPointsEarned.Single().TotalTransactionalAmount;
        //            customerLoyaltyPoint.CumulativePoints += (recordCount * loyaltyTransactionAmountSchemeBand.NumberOfLoyaltyPoints);
        //        }
        //        else
        //        {
        //            customerLoyaltyPoint = new CustomerLoyaltyPoint
        //            {
        //                OrganizationId = botLoyaltyRecord.OrganizationId,
        //                PaymentInstrumentId = (long)botLoyaltyRecord.SourcePaymentInstrumentId,
        //                //TransactionFrequency = initialTransactionFrequency,
        //                CumulativeFeeAmount = loyaltyPointsEarned.Single().TotalFeeAmount,
        //                CumulativeTransactionAmount = loyaltyPointsEarned.Single().TotalTransactionalAmount,
        //                CumulativePoints = (recordCount * loyaltyTransactionAmountSchemeBand.NumberOfLoyaltyPoints)
        //            };

        //            context.CustomerLoyaltyPoints.Add(customerLoyaltyPoint);
        //        }
        //        context.SaveChanges();

        //        sourcePaymentInstrument.LoyaltyPointBalance = botLoyaltyRecord.OrganizationId == (int)Organizations.ILoyal ? customerLoyaltyPoint.CumulativePoints : 0;
        //        toProcessBotRecords.ForEach(record => record.IsLoyaltyBasedOnAmountSchemeExecuted = true);
        //        context.SaveChanges();

        //        return "Success";

        //    }
        //    catch (Exception ex)
        //    {
        //        messageRepository.GenericExceptionEmail("info@iloyal.com", "Error: Processing Loyalty Points By Amount", string.Format("BOT EXECUTION RECORD BY AMOUNT-SCHEME EXCEPTION<br><br>{0}<br><br> Stack trace<br><br>{1}", ex.Message, ex.StackTrace), string.Empty);
        //        throw;
        //    }
        //}

        //public string ProcessBotExecutionRecordByFrequencyScheme(LoyaltyPointExecutionBot botLoyaltyRecord)
        //{
        //    try
        //    {
        //        if (botLoyaltyRecord == null)
        //            return null;

        //        PaymentInstrument sourcePaymentInstrument = context.PaymentInstruments.Find(botLoyaltyRecord.SourcePaymentInstrumentId);

        //        if (sourcePaymentInstrument == null)
        //            return null;

        //        LoyaltyTransactionFrequencySchemeDto loyaltyTransactionFrequencySchemeBand = GetLoyaltyTransactionFrequencyScheme(botLoyaltyRecord.OrganizationId, botLoyaltyRecord.TransactionTypeId, sourcePaymentInstrument.PaymentInstrumentTypeId);

        //        if (loyaltyTransactionFrequencySchemeBand == null)
        //            return null;

        //        if (!loyaltyTransactionFrequencySchemeBand.IsEligibleToAccruePoints)
        //            return null;

        //        List<LoyaltyPointExecutionBot> toProcessBotRecords = context.LoyaltyPointExecutionBots.Where(bot => bot.SourceCustomerId == botLoyaltyRecord.SourceCustomerId && bot.SourcePaymentInstrumentId == botLoyaltyRecord.SourcePaymentInstrumentId && bot.OrganizationId == botLoyaltyRecord.OrganizationId && bot.TransactionTypeId == botLoyaltyRecord.TransactionTypeId && bot.IsLoyaltyBasedOnFrequencySchemeExecuted == false).ToList();

        //        int recordCount = toProcessBotRecords.Count;
        //        if (recordCount <= 0)
        //            return null;

        //        //do not take the load out of the method and be precise and take the least computation time possible --remember the loyalty point summations should also reflect in the PI-for MK only
        //        //think of efficiency and turn around time + deadloacks - optimistic locking (lol)-- since we cant have same payer doing 2 transactions simultaneously - no brain division
        //        //take advantage of the Math lib resource as did with commmissions (Math.Div)

        //        int quotient = 0;
        //        int remainder = 0;

        //        quotient = Math.DivRem(recordCount, loyaltyTransactionFrequencySchemeBand.NumberOfActivities, out remainder);

        //        if (quotient <= 0) // means no successful batches are formed
        //            return null;

        //        List<LoyaltyPointExecutionBot> toUpdateBotRecords = toProcessBotRecords.OrderBy(order => order.LoyaltyPointExecutionBotId)
        //                                                            .Take(quotient * loyaltyTransactionFrequencySchemeBand.NumberOfActivities).ToList();

        //        var loyaltyPointsEarned = toUpdateBotRecords.GroupBy(p => new { p.OrganizationId, p.SourceCustomerId, p.SourcePaymentInstrumentId, p.TransactionTypeId })
        //                                    .Select(x => new
        //                                    {
        //                                        TotalFeeAmount = x.Sum(g => g.TransactionFee),
        //                                        TotalTransactionalAmount = x.Sum(g => g.TransactionAmount)
        //                                    });

        //        CustomerLoyaltyPoint customerLoyaltyPoint = context.CustomerLoyaltyPoints.SingleOrDefault(x => x.OrganizationId == botLoyaltyRecord.OrganizationId && x.PaymentInstrumentId == (long)botLoyaltyRecord.SourcePaymentInstrumentId);

        //        if (customerLoyaltyPoint != null)
        //        {
        //            //customerLoyaltyPoint.TransactionFrequency += (quotient * loyaltyTransactionFrequencySchemeBand.NumberOfActivities);
        //            customerLoyaltyPoint.CumulativeFeeAmount += loyaltyPointsEarned.Single().TotalFeeAmount;
        //            customerLoyaltyPoint.CumulativeTransactionAmount += loyaltyPointsEarned.Single().TotalTransactionalAmount;
        //            customerLoyaltyPoint.CumulativePoints += (quotient * loyaltyTransactionFrequencySchemeBand.NumberOfLoyaltyPoints);
        //        }
        //        else
        //        {
        //            customerLoyaltyPoint = new CustomerLoyaltyPoint
        //            {
        //                OrganizationId = botLoyaltyRecord.OrganizationId,
        //                PaymentInstrumentId = (long)botLoyaltyRecord.SourcePaymentInstrumentId,
        //                //TransactionFrequency = (quotient * loyaltyTransactionFrequencySchemeBand.NumberOfActivities),
        //                CumulativeFeeAmount = loyaltyPointsEarned.Single().TotalFeeAmount,
        //                CumulativeTransactionAmount = loyaltyPointsEarned.Single().TotalTransactionalAmount,
        //                CumulativePoints = (quotient * loyaltyTransactionFrequencySchemeBand.NumberOfLoyaltyPoints)
        //            };

        //            context.CustomerLoyaltyPoints.Add(customerLoyaltyPoint);
        //        }
        //        context.SaveChanges();

        //        sourcePaymentInstrument.LoyaltyPointBalance = botLoyaltyRecord.OrganizationId == (int)Organizations.ILoyal ? customerLoyaltyPoint.CumulativePoints : 0;
        //        toUpdateBotRecords.ForEach(record => record.IsLoyaltyBasedOnFrequencySchemeExecuted = true);
        //        context.SaveChanges();

        //        return "Success";
        //    }
        //    catch (Exception ex)
        //    {
        //        messageRepository.GenericExceptionEmail("wallet@directcore.com", "Error: Processing Loyalty Points By Amount", string.Format("BOT EXECUTION RECORD BY AMOUNT-SCHEME EXCEPTION<br><br>{0}<br><br> Stack trace<br><br>{1}", ex.Message, ex.StackTrace), string.Empty);
        //        throw;
        //    }
        //}

        //public string ProcessBotRecordByCumulativeTransactionAmountScheme(LoyaltyPointExecutionBot botLoyaltyRecord)
        //{
        //    try
        //    {
        //        const long resetValue = 0;
        //        if (botLoyaltyRecord == null)
        //            return null;
        //        PaymentInstrument sourcePaymentInstrument = context.PaymentInstruments.Find(botLoyaltyRecord.SourcePaymentInstrumentId);

        //        if (sourcePaymentInstrument == null)
        //            return null;

        //        //Also takes care of the transactiontyes that can be executed using this scheme
        //        LoyaltyCumulativeTransactionAmountSchemeDto cumulativeTransactionAmountScheme = GetLoyaltyCumulativeTransactionAmountScheme(botLoyaltyRecord.OrganizationId, botLoyaltyRecord.TransactionTypeId, sourcePaymentInstrument.PaymentInstrumentTypeId);

        //        if (cumulativeTransactionAmountScheme == null)
        //            return null;

        //        if (!cumulativeTransactionAmountScheme.IsEligibleToAccruePoints)
        //            return null;

        //        LoyaltyCumulativeAmountSchemeTag cumulativeAmountSchemeTag = context.LoyaltyCumulativeAmountSchemeTags.SingleOrDefault(tag => tag.OrganizationId == botLoyaltyRecord.OrganizationId && tag.PaymentInstrumentId == (long)botLoyaltyRecord.SourcePaymentInstrumentId && tag.TransactionTypeId == botLoyaltyRecord.TransactionTypeId);

        //        if (cumulativeAmountSchemeTag != null)
        //        {
        //            cumulativeAmountSchemeTag.TransactionCount += initialTransactionFrequency;
        //            cumulativeAmountSchemeTag.ComputedBalance += botLoyaltyRecord.TransactionAmount;
        //            cumulativeAmountSchemeTag.Amount += botLoyaltyRecord.TransactionAmount;
        //            cumulativeAmountSchemeTag.Fee += botLoyaltyRecord.TransactionFee;
        //        }
        //        else
        //        {
        //            cumulativeAmountSchemeTag = new LoyaltyCumulativeAmountSchemeTag
        //            {
        //                PaymentInstrumentId = (long)botLoyaltyRecord.SourcePaymentInstrumentId,
        //                TransactionTypeId = botLoyaltyRecord.TransactionTypeId,
        //                OrganizationId = botLoyaltyRecord.OrganizationId,
        //                Amount = botLoyaltyRecord.TransactionAmount,
        //                Fee = botLoyaltyRecord.TransactionFee,
        //                ComputedBalance = botLoyaltyRecord.TransactionAmount,
        //                TransactionCount = initialTransactionFrequency
        //            };
        //            context.LoyaltyCumulativeAmountSchemeTags.Add(cumulativeAmountSchemeTag);
        //        }

        //        //update the loyalty execution bot record - we cant use cumulated amounts and frequency at same time to avoud cheating the system              
        //        botLoyaltyRecord.IsLoyaltyBasedOnCumulativeAmountSchemeExecuted = true;
        //        context.SaveChanges();

        //        long quotient = resetValue;
        //        long remainder = resetValue;

        //        quotient = Math.DivRem(cumulativeAmountSchemeTag.ComputedBalance, cumulativeTransactionAmountScheme.NetTransactionAmount, out remainder);

        //        if (quotient <= resetValue)
        //            return null;

        //        CustomerLoyaltyPoint customerLoyaltyPoint = context.CustomerLoyaltyPoints.SingleOrDefault(x => x.OrganizationId == botLoyaltyRecord.OrganizationId && x.PaymentInstrumentId == (long)botLoyaltyRecord.SourcePaymentInstrumentId);

        //        if (customerLoyaltyPoint != null)
        //        {
        //            //customerLoyaltyPoint.TransactionFrequency += cumulativeAmountSchemeTag.TransactionCount;
        //            customerLoyaltyPoint.CumulativeFeeAmount += cumulativeAmountSchemeTag.Fee;
        //            customerLoyaltyPoint.CumulativeTransactionAmount += cumulativeAmountSchemeTag.Amount;
        //            customerLoyaltyPoint.CumulativePoints += (quotient * cumulativeTransactionAmountScheme.NumberOfLoyaltyPoints);
        //        }
        //        else
        //        {
        //            customerLoyaltyPoint = new CustomerLoyaltyPoint
        //            {
        //                OrganizationId = botLoyaltyRecord.OrganizationId,
        //                PaymentInstrumentId = (long)botLoyaltyRecord.SourcePaymentInstrumentId,
        //                //TransactionFrequency = cumulativeAmountSchemeTag.TransactionCount,
        //                CumulativeFeeAmount = cumulativeAmountSchemeTag.Fee,
        //                CumulativeTransactionAmount = cumulativeAmountSchemeTag.Amount,
        //                CumulativePoints = (quotient * cumulativeTransactionAmountScheme.NumberOfLoyaltyPoints)
        //            };

        //            context.CustomerLoyaltyPoints.Add(customerLoyaltyPoint);
        //        }

        //        sourcePaymentInstrument.LoyaltyPointBalance = botLoyaltyRecord.OrganizationId == (int)Organizations.ILoyal ? customerLoyaltyPoint.CumulativePoints : resetValue;
        //        cumulativeAmountSchemeTag.ComputedBalance = remainder;
        //        cumulativeAmountSchemeTag.TransactionCount = resetValue;
        //        cumulativeAmountSchemeTag.Fee = resetValue;
        //        cumulativeAmountSchemeTag.Amount = resetValue;

        //        context.SaveChanges();

        //        return "Success";
        //    }
        //    catch (Exception ex)
        //    {
        //        messageRepository.GenericExceptionEmail("wallet@directcore.com", "Error: Processing Loyalty Points By Amount", string.Format("BOT EXECUTION RECORD BY AMOUNT-SCHEME EXCEPTION<br><br>{0}<br><br> Stack trace<br><br>{1}", ex.Message, ex.StackTrace), string.Empty);
        //        throw;
        //    }
        //}

        //public LoyaltyTransactionAmountSchemeDto CreateLoyaltyAmountSchemeBand(int organizationId, int transactionTypeId, int paymentInstrumentTypeId, bool isActive, string transactionDescription)
        //{
        //    try
        //    {
        //        Organization organization = context.Organizations.Find(organizationId);
        //        if (organization == null)
        //            throw new Exception(string.Format("GN0008 - The specified OrganizationId '{0}' does not exist.", organizationId));

        //        TransactionType transactionType = context.TransactionTypes.Find(transactionTypeId);
        //        if (transactionType == null)
        //            throw new Exception(string.Format("TRN0024 - The specified TransactionTypeId '{0}' does not exist.", transactionTypeId));

        //        PaymentInstrumentType paymentInstrumentType = context.PaymentInstrumentTypes.Find(paymentInstrumentTypeId);
        //        if (paymentInstrumentType == null)
        //            throw new Exception(string.Format("PI0017 - The PaymentInstrumentTypeId '{0}' does not exist.", paymentInstrumentTypeId));

        //        LoyaltyTransactionAmountScheme amountScheme = new LoyaltyTransactionAmountScheme
        //        {
        //            OrganizationId = organizationId,
        //            TransactionTypeId = transactionTypeId,
        //            PaymentInstrumentTypeId = paymentInstrumentTypeId,
        //            SenderEarnsPoints = true,
        //            RecipientEarnsPoints = false,
        //            TransactionDescription = string.IsNullOrEmpty(transactionDescription) ? null : transactionDescription,
        //            IsEligibleToAccruePoints = isActive
        //        };

        //        context.LoyaltyTransactionAmountSchemes.Add(amountScheme);
        //        context.SaveChanges();

        //        return new LoyaltyTransactionAmountSchemeDto { IsEligibleToAccruePoints = amountScheme.IsEligibleToAccruePoints, LoyaltyTransactionAmountSchemeId = amountScheme.LoyaltyTransactionAmountSchemeId, NumberOfActivities = amountScheme.NumberOfActivities, NumberOfLoyaltyPoints = amountScheme.NumberOfLoyaltyPoints, OrganizationId = amountScheme.OrganizationId, PaymentInstrumentTypeId = amountScheme.PaymentInstrumentTypeId, RecipientEarnsPoints = amountScheme.RecipientEarnsPoints, SenderEarnsPoints = amountScheme.SenderEarnsPoints, TransactionDescription = amountScheme.TransactionDescription, TransactionTypeId = amountScheme.TransactionTypeId };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //public LoyaltyTransactionAmountSchemeItemDto CreateLoyaltyAmountSchemeBandItem(long loyaltyTransactionAmountSchemeId, long minimumValue, long maximumValue, int numberOfPointsBooked)
        //{
        //    try
        //    {
        //        LoyaltyTransactionAmountScheme amountScheme = context.LoyaltyTransactionAmountSchemes.Find(loyaltyTransactionAmountSchemeId);
        //        if (amountScheme == null)
        //            throw new Exception(string.Format("LY0009 - The LoyaltyTransactionAmountSchemeId '{0}' does not exist.", loyaltyTransactionAmountSchemeId));

        //        LoyaltyTransactionAmountSchemeItem schemeItem = new LoyaltyTransactionAmountSchemeItem
        //        {
        //            LoyaltyTransactionAmountSchemeId = loyaltyTransactionAmountSchemeId,
        //            MinimumValue = minimumValue,
        //            MaximumValue = maximumValue,
        //            NumberOfLoyaltyPoints = numberOfPointsBooked
        //        };

        //        context.LoyaltyTransactionAmountSchemeItems.Add(schemeItem);
        //        context.SaveChanges();

        //        return new DTO.LoyaltyTransactionAmountSchemeItemDto { LoyaltyTransactionAmountSchemeId = schemeItem.LoyaltyTransactionAmountSchemeId, LoyaltyTransactionAmountSchemeItemId = schemeItem.LoyaltyTransactionAmountSchemeItemId, MaximumValue = schemeItem.MaximumValue, MinimumValue = schemeItem.MinimumValue, NumberOfLoyaltyPoints = schemeItem.NumberOfLoyaltyPoints };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //public LoyaltyCumulativeTransactionAmountSchemeDto CreateLoyaltyCumulativeTransactionAmountSchemeBand(int organizationId, int transactionTypeId, int paymentInstrumentTypeId, long netTransactionAmount, int numberofPointsBooked, bool isActive, string transactionDescription)
        //{
        //    try
        //    {
        //        Organization organization = context.Organizations.Find(organizationId);
        //        if (organization == null)
        //            throw new Exception(string.Format("GN0008 - The specified OrganizationId '{0}' does not exist.", organizationId));

        //        TransactionType transactionType = context.TransactionTypes.Find(transactionTypeId);
        //        if (transactionType == null)
        //            throw new Exception(string.Format("TRN0024 - The specified TransactionTypeId '{0}' does not exist.", transactionTypeId));

        //        PaymentInstrumentType paymentInstrumentType = context.PaymentInstrumentTypes.Find(paymentInstrumentTypeId);
        //        if (paymentInstrumentType == null)
        //            throw new Exception(string.Format("PI0017 - The PaymentInstrumentTypeId '{0}' does not exist.", paymentInstrumentTypeId));

        //        LoyaltyCumulativeTransactionAmountScheme cumulativeAmountScheme = new LoyaltyCumulativeTransactionAmountScheme
        //        {
        //            OrganizationId = organizationId,
        //            TransactionTypeId = transactionTypeId,
        //            PaymentInstrumentTypeId = paymentInstrumentTypeId,
        //            NetTransactionAmount = netTransactionAmount,
        //            NumberOfLoyaltyPoints = numberofPointsBooked,
        //            SenderEarnsPoints = true,
        //            RecipientEarnsPoints = false,
        //            TransactionDescription = string.IsNullOrEmpty(transactionDescription) ? null : transactionDescription,
        //            IsEligibleToAccruePoints = isActive
        //        };

        //        context.LoyaltyCumulativeTransactionAmountSchemes.Add(cumulativeAmountScheme);
        //        context.SaveChanges();

        //        return new LoyaltyCumulativeTransactionAmountSchemeDto { NumberOfLoyaltyPoints = cumulativeAmountScheme.NumberOfLoyaltyPoints, IsEligibleToAccruePoints = cumulativeAmountScheme.IsEligibleToAccruePoints, LoyaltyCumulativeTransactionAmountSchemeId = cumulativeAmountScheme.LoyaltyCumulativeTransactionAmountSchemeId, NetTransactionAmount = cumulativeAmountScheme.NetTransactionAmount, OrganizationId = cumulativeAmountScheme.OrganizationId, PaymentInstrumentTypeId = cumulativeAmountScheme.PaymentInstrumentTypeId, RecipientEarnsPoints = cumulativeAmountScheme.RecipientEarnsPoints, SenderEarnsPoints = cumulativeAmountScheme.SenderEarnsPoints, TransactionDescription = cumulativeAmountScheme.TransactionDescription, TransactionTypeId = cumulativeAmountScheme.TransactionTypeId };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //public LoyaltyTransactionFrequencySchemeDto CreateLoyaltyFrequencySchemeBand(int organizationId, int transactionTypeId, int paymentInstrumentTypeId, int numberOfTransactions, int numberofPointsBooked, bool isActive, string transactionDescription)
        //{
        //    try
        //    {
        //        Organization organization = context.Organizations.Find(organizationId);
        //        if (organization == null)
        //            throw new Exception(string.Format("GN0008 - The specified OrganizationId '{0}' does not exist.", organizationId));

        //        TransactionType transactionType = context.TransactionTypes.Find(transactionTypeId);
        //        if (transactionType == null)
        //            throw new Exception(string.Format("TRN0024 - The specified TransactionTypeId '{0}' does not exist.", transactionTypeId));

        //        PaymentInstrumentType paymentInstrumentType = context.PaymentInstrumentTypes.Find(paymentInstrumentTypeId);
        //        if (paymentInstrumentType == null)
        //            throw new Exception(string.Format("PI0017 - The PaymentInstrumentTypeId '{0}' does not exist.", paymentInstrumentTypeId));

        //        LoyaltyTransactionFrequencyScheme frequencyScheme = new LoyaltyTransactionFrequencyScheme
        //        {
        //            OrganizationId = organizationId,
        //            TransactionTypeId = transactionTypeId,
        //            PaymentInstrumentTypeId = paymentInstrumentTypeId,
        //            NumberOfActivities = numberOfTransactions,
        //            NumberOfLoyaltyPoints = numberofPointsBooked,
        //            SenderEarnsPoints = true,
        //            RecipientEarnsPoints = false,
        //            TransactionDescription = string.IsNullOrEmpty(transactionDescription) ? null : transactionDescription,
        //            IsEligibleToAccruePoints = isActive
        //        };

        //        context.LoyaltyTransactionFrequencySchemes.Add(frequencyScheme);
        //        context.SaveChanges();

        //        return new LoyaltyTransactionFrequencySchemeDto { OrganizationId = frequencyScheme.OrganizationId, IsEligibleToAccruePoints = frequencyScheme.IsEligibleToAccruePoints, LoyaltyTransactionFrequencySchemeId = frequencyScheme.LoyaltyTransactionFrequencySchemeId, NumberOfActivities = frequencyScheme.NumberOfActivities, NumberOfLoyaltyPoints = frequencyScheme.NumberOfLoyaltyPoints, PaymentInstrumentTypeId = frequencyScheme.PaymentInstrumentTypeId, RecipientEarnsPoints = frequencyScheme.RecipientEarnsPoints, SenderEarnsPoints = frequencyScheme.SenderEarnsPoints, TransactionDescription = frequencyScheme.TransactionDescription, TransactionTypeId = frequencyScheme.TransactionTypeId };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //private CustomerLoyaltyPoint GetCustomerLoyaltyPoints(int organizationId, long paymentInstrumentId)
        //{
        //    try
        //    {
        //        CustomerLoyaltyPoint customerLoyaltyPoint = context.CustomerLoyaltyPoints.SingleOrDefault(x => x.OrganizationId == organizationId && x.PaymentInstrumentId == paymentInstrumentId);

        //        return customerLoyaltyPoint;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}


        //private LoyaltyCumulativeAmountSchemeTag GetLoyaltyCumulativeAmountSchemeTag(int organizationId, long paymentInstrumentId, int transactionTypeId)
        //{
        //    try
        //    {
        //        LoyaltyCumulativeAmountSchemeTag cumulativeAmountSchemeTag = context.LoyaltyCumulativeAmountSchemeTags.SingleOrDefault(tag => tag.OrganizationId == organizationId && tag.PaymentInstrumentId == paymentInstrumentId && tag.TransactionTypeId == transactionTypeId);

        //        return cumulativeAmountSchemeTag;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //private bool UpdateSuccessfullyProcessedBotRecord(LoyaltyPointExecutionBot botRecordToUpdate, bool isUpdatingByFrequencyScheme)
        //{

        //    try
        //    {
        //        if (botRecordToUpdate == null)
        //        {
        //            throw new Exception("LY0005 - A valid LoyaltyExecustionBot Record must be provided for update.");
        //        }

        //        if (isUpdatingByFrequencyScheme)
        //        {
        //            botRecordToUpdate.IsLoyaltyBasedOnFrequencySchemeExecuted = true;
        //        }
        //        else
        //        {
        //            botRecordToUpdate.IsLoyaltyBasedOnAmountSchemeExecuted = true;
        //        }
        //        context.SaveChanges();
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public void UpdateExecutionBotLoyaltyPoint(LoyaltyPointExecutionBot botLoyaltyRecord)
        //{

        //    if (botLoyaltyRecord == null)
        //        return;

        //    try
        //    {
        //        //call the 2 methods asynchronously
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //}

        //public string ValidatePointRedemption(long paymentInstrumentId, long inAccountLoyaltyPoints, long redemptionPoints)
        //{
        //    //Any Customer may only redeem the points above the collateral figure for Sawa Loan            
        //    List<StandingOrder> standingOrder = context.StandingOrders.Where(order => order.PaymentInstrumentId == paymentInstrumentId && order.StandingOrderStatusId != (int)Enums.StandingOrderStatus.Completed && order.StandingOrderStatusId != (int)Enums.StandingOrderStatus.InactiveCancelled).ToList();

        //    if (standingOrder == null)
        //        return null;

        //    long collateralPoints = 0;

        //    collateralPoints = standingOrder.Sum(order => order.EquivalentLoyaltyPoints);

        //    long redeemableValuation = inAccountLoyaltyPoints - collateralPoints;

        //    if (redeemableValuation < redemptionPoints)
        //    {
        //        throw new Exception(string.Format("LY0009 - The PaymentInstrumentId '{0}' does not have sufficient Loyalty Points for this transaction. Usable points after Collateral are '{1}'", paymentInstrumentId, redeemableValuation));
        //    }
        //    else { return "Success"; }
        //}

        //public long AvailableLoyaltyPointBalance(long paymentInstrumentId, out long currentLoyaltyPointBalance)
        //{
        //    currentLoyaltyPointBalance = 0;
        //    try
        //    {
        //        long collateralPoints = 0;

        //        List<StandingOrder> standingOrderList = context.StandingOrders.Where(order => order.PaymentInstrumentId == paymentInstrumentId && order.StandingOrderStatusId != (int)Enums.StandingOrderStatus.Completed && order.StandingOrderStatusId != (int)Enums.StandingOrderStatus.InactiveCancelled).ToList();
        //        if (standingOrderList.Count > 0)
        //        {
        //            collateralPoints = standingOrderList.Sum(x => x.EquivalentLoyaltyPoints);
        //        }

        //        PaymentInstrument paymentInstrument = context.PaymentInstruments.Find(paymentInstrumentId);
        //        if (paymentInstrument == null) { return 0; }

        //        currentLoyaltyPointBalance = paymentInstrument.LoyaltyPointBalance;
        //        long availablePointsBalance = paymentInstrument.LoyaltyPointBalance - collateralPoints;
        //        return availablePointsBalance;
        //    }
        //    catch
        //    {
        //        return 0;
        //    }
        //}

        //public bool LoyaltySensitizationSmsBroadCast()
        //{
        //    try
        //    {
        //        var query = (from pi in context.PaymentInstruments.Where(x => x.PaymentInstrumentTypeId == 1 && x.IsActive == true && x.Customer.CustomerTypeId == 2).Except(context.StandingOrders.Where(x => x.StandingOrderTypeId == 2 && x.StandingOrderStatusId != 3 && x.StandingOrderStatusId != 4).Select(x => x.PaymentInstrument).Distinct())
        //                     join cust in context.CustomerMsisdns on pi.CustomerId equals cust.CustomerId
        //                     join msisdn in context.MasterMsisdnLogs on cust.MasterMsidnLogId equals msisdn.MasterMsisdnLogId
        //                     orderby pi.LoyaltyPointBalance descending
        //                     select new { Msisdn = msisdn.Msisdn, Points = pi.LoyaltyPointBalance, AmountQualified = (pi.LoyaltyPointBalance * 2000) });//.Take(20000);

        //        messageRepository.SawaLoanSensitizationSms(query);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //        throw new Exception(ex.Message);
        //    }
        //}

        //public void ReprocessSensitization()
        //{
        //    //1469998 > 1454339
        //    //messageRepository.ReprocessSensitization();
        //}

        //public bool PopulateLoyaltyFrequencyScheme()
        //{
        //    try
        //    {
        //        List<LoyaltySetup> allSchemes = context.LoyaltySetups.ToList();
        //        List<LoyaltyTransactionFrequencyScheme> trxnScheme = new List<LoyaltyTransactionFrequencyScheme>();
        //        allSchemes.ForEach(x => trxnScheme.Add(new LoyaltyTransactionFrequencyScheme
        //        {
        //            OrganizationId = (int)Organizations.ILoyal,
        //            TransactionTypeId = x.TransactionTypeId,
        //            PaymentInstrumentTypeId = x.PaymentInstrumentTypeId,
        //            NumberOfActivities = x.NumberOfActivities,
        //            NumberOfLoyaltyPoints = x.NumberOfLoyaltyPoints,
        //            SenderEarnsPoints = true,
        //            RecipientEarnsPoints = false,
        //            TransactionDescription = x.TransactionDescription,
        //            IsEligibleToAccruePoints = (x.TransactionTypeId == (int)TransactionTypes.AirtimeTopup || x.TransactionTypeId == (int)TransactionTypes.AirtimeSale) ? false : true
        //        }));

        //        context.LoyaltyTransactionFrequencySchemes.AddRange(trxnScheme);
        //        context.SaveChanges();

        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

    }
}