using System;
using System.Collections.Generic;
using System.Linq;

using Dapper;

using Hook.Models;

using System.Configuration;
using System.Data.SqlClient;

namespace Hook.Repository
{
    public class SetUpRepository
    {
        string sqlConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString();

        public int CreateCustomerType(string customerTypeName, string customerTypeDescription)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                var affectedRows = connection.Execute("Insert into CustomerType (CustomerTypeName,CustomerTypeDescription) values (@CustomerTypeName, @CustomerTypeDescription)", new { customerTypeName, customerTypeDescription });

                connection.Close();

                return affectedRows;
            }
        }

        public int UpdateCustomerType(long customerTypeId, string customerTypeName, string customerTypeDescription)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("UPDATE CustomerType SET CustomerTypeName=@CustomerTypeName,CustomerTypeDescription=@CustomerTypeDescription WHERE CustomerTypeId=@CustomerTypeId", new { CustomerTypeId = customerTypeId, CustomerTypeName = customerTypeName, CustomerTypeDescription = customerTypeDescription });

                connection.Close();

                return affectedRows;
            }
        }

        public CustomerType GetCustomerTypeByCustomerTypeId(long customerTypeId)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                return connection.Query<CustomerType>("SELECT * FROM CustomerType WHERE CustomerTypeId=@CustomerTypeId", new { CustomerTypeId = customerTypeId }).SingleOrDefault();
            }
        }

        public List<CustomerType> GetAllCustomerTypes()
        {
            List<CustomerType> customerTypes = new List<CustomerType>();
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                customerTypes = connection.Query<CustomerType>("Select * FROM CustomerType").ToList();

                connection.Close();
            }

            return customerTypes;
        }

        public int CreateIDType(string idTypeName, string idTypeDescription)
        {

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                var affectedRows = connection.Execute("Insert into IdType (IdTypeName, IdTypeDescription) values (@IdTypeName, @IdTypeDescription)", new { idTypeName, idTypeDescription });

                connection.Close();

                return affectedRows;
            }
        }

        public int UpdateIDType(long idTypeId, string idTypeName, string idTypeDescription)
        {

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("UPDATE IdType SET IdTypeName=@IdTypeName,IdTypeDescription=@IdTypeDescription WHERE IdTypeId=@IdTypeId", new { IdTypeId = idTypeId, IdTypeName = idTypeName, IdTypeDescription = idTypeDescription });

                connection.Close();

                return affectedRows;
            }
        }

        public IDType GetIDTypeByIDTypeId(long idTypeId)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                return connection.Query<IDType>("SELECT * FROM IdType WHERE IdTypeId=@IdTypeId", new { IdTypeId = idTypeId }).SingleOrDefault();
            }

        }

        public List<IDType> GetAllIDTypes()
        {
            List<IDType> idTypes = new List<IDType>();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                idTypes = connection.Query<IDType>("Select * FROM IdType").ToList();

                connection.Close();
            }

            return idTypes;
        }

        public int CreateUserType(string userTypeName, string userTypeDescription, string createdBy)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                var affectedRows = connection.Execute("Insert into UserType (UserTypeName, UserTypeDescription) values (@UserTypeName, @UserTypeDescription)", new { userTypeName, userTypeDescription });

                connection.Close();

                return affectedRows;
            }
        }

        public int UpdateUserType(long userTypeId, string userTypeName, string userTypeDescription)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("UPDATE UserType SET UserTypeName=@UserTypeName,UserTypeDescription=@UserTypeDescription WHERE UserTypeId=@UserTypeId", new { UserTypeId = userTypeId, UserTypeName = userTypeName, UserTypeDescription = userTypeDescription });

                connection.Close();

                return affectedRows;
            }
        }

        public UserType GetUserTypeByUserTypeId(long userTypeId)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                return connection.Query<UserType>("SELECT * FROM UserType WHERE UserTypeId=@UserTypeId", new { UserTypeId = userTypeId }).SingleOrDefault();
            }
        }

        public List<UserType> GetAllUserTypes()
        {
            List<UserType> userTypes = new List<UserType>();
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                userTypes = connection.Query<UserType>("Select * FROM UserType").ToList();

                connection.Close();
            }
            return userTypes;
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