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

        public int CreateCustomerType(string customerTypeName, string customerTypeDescription, string createdBy)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                var affectedRows = connection.Execute("Insert into CustomerType (CustomerTypeName,CustomerTypeDescription, CreatedBy, CreatedDate) values (@CustomerTypeName, @CustomerTypeDescription, @CreatedBy, @CreatedDate)", new { customerTypeName, customerTypeDescription, createdBy, CreatedDate = GetRealDate() });

                connection.Close();

                return affectedRows;
            }
        }

        public int UpdateCustomerType(long customerTypeId, string customerTypeName, string customerTypeDescription, string updatedBy)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("UPDATE CustomerType SET CustomerTypeName=@CustomerTypeName,CustomerTypeDescription=@CustomerTypeDescription, UpdatedBy=@UpdatedBy, UpdatedDate=@UpdatedDate WHERE CustomerTypeId=@CustomerTypeId", new { CustomerTypeId = customerTypeId, CustomerTypeName = customerTypeName, CustomerTypeDescription = customerTypeDescription, UpdatedBy = updatedBy, UpdatedDate = GetRealDate() });

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

        public int CreateTown(string townName, string townDescription, long subCountyId, string createdBy)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                var affectedRows = connection.Execute("Insert into Town (TownName, TownDescription, SubCountyId, CreatedBy, CreatedDate) values (@TownName, @TownDescription, @SubCountyId, @CreatedBy, @CreatedDate)", new { townName, townDescription, subCountyId, createdBy, CreatedDate = GetRealDate() });

                connection.Close();

                return affectedRows;
            }
        }

        public int UpdateTown(long townId, string townName, string townDescription, long subCountyId, string updatedBy)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("UPDATE Town SET TownName=@TownName,TownDescription=@TownDescription, SubCountyId=@SubCountyId, UpdatedBy=@UpdatedBy, UpdatedDate=@UpdatedDate WHERE TownId=@TownId", new { TownId = townId, TownName = townName, TownDescription = townDescription, SubCountyId = subCountyId, UpdatedBy = updatedBy, UpdatedDate = GetRealDate() });

                connection.Close();

                return affectedRows;
            }
        }

        public Town GetTownByTownId(long townId)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                return connection.Query<Town>("SELECT * FROM Town WHERE TownId=@TownId", new { TownId = townId }).SingleOrDefault();
            }
        }


        public List<Town> GetTownsBySubCountyId(long subCountyId)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                return connection.Query<Town>("SELECT * FROM Town WHERE SubCountyId=@SubCountyId", new { SubCountyId = subCountyId }).ToList();
            }
        }

        public List<Town> GetAllTowns()
        {
            List<Town> towns = new List<Town>();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                towns = connection.Query<Town>("Select * FROM Town").ToList();

                connection.Close();
            }

            return towns;
        }


        public int CreateSubCounty(string subCountyName, string subCountyDescription, string createdBy)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                var affectedRows = connection.Execute("Insert into SubCounty (SubCountyName, SubCountyDescription, CreatedBy, CreatedDate) values (@SubCountyName, @SubCountyDescription, @CreatedBy, @CreatedDate)", new { subCountyName, subCountyDescription, createdBy, CreatedDate = GetRealDate() });

                connection.Close();

                return affectedRows;
            }
        }

        public int UpdateSubCounty(long subCountyId, string subCountyName, string subCountyDescription, string updatedBy)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("UPDATE SubCounty SET SubCountyName=@SubCountyName,SubCountyDescription=@SubCountyDescription, UpdatedBy=@UpdatedBy, UpdatedDate=@UpdatedDate WHERE SubCountyId=@SubCountyId", new { SubCountyId = subCountyId, SubCountyName = subCountyName, SubCountyDescription = subCountyDescription, UpdatedBy = updatedBy, UpdatedDate = GetRealDate() });

                connection.Close();

                return affectedRows;
            }
        }

        public SubCounty GetSubCountyBySubCountyId(long subCountyId)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                return connection.Query<SubCounty>("SELECT * FROM SubCounty WHERE SubCountyId=@SubCountyId", new { SubCountyId = subCountyId }).SingleOrDefault();
            }
        }

        public List<SubCounty> GetAllSubCounties()
        {
            List<SubCounty> subCounties = new List<SubCounty>();
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                subCounties = connection.Query<SubCounty>("Select * FROM SubCounty").ToList();

                connection.Close();
            }
            return subCounties;
        }


        public int CreateIDType(string idTypeName, string idTypeDescription, string createdBy)
        {

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                var affectedRows = connection.Execute("Insert into IdType (IdTypeName, IdTypeDescription, CreatedBy, CreatedDate) values (@IdTypeName, @IdTypeDescription, @CreatedBy, @CreatedDate)", new { idTypeName, idTypeDescription, createdBy, CreatedDate = GetRealDate() });

                connection.Close();

                return affectedRows;
            }
        }

        public int UpdateIDType(long idTypeId, string idTypeName, string idTypeDescription, string updatedBy)
        {

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("UPDATE IdType SET IdTypeName=@IdTypeName,IdTypeDescription=@IdTypeName, UpdatedBy=@UpdatedBy, UpdatedDate=@UpdatedDate WHERE IdTypeId=@IdTypeId", new { IdTypeId = idTypeId, IdTypeName = idTypeName, IdTypeDescription = idTypeDescription, UpdatedBy = updatedBy, UpdatedDate = GetRealDate() });

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

                var affectedRows = connection.Execute("Insert into UserType (UserTypeName, UserTypeDescription, CreatedBy, CreatedDate) values (@UserTypeName, @UserTypeDescription, @CreatedBy, @CreatedDate)", new { userTypeName, userTypeDescription, createdBy, CreatedDate = GetRealDate() });

                connection.Close();

                return affectedRows;
            }
        }

        public int UpdateUserType(long userTypeId, string userTypeName, string userTypeDescription, string updatedBy)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var affectedRows = connection.Execute("UPDATE UserType SET UserTypeName=@UserTypeName,UserTypeDescription=@UserTypeDescription, UpdatedBy=@UpdatedBy, UpdatedDate=@UpdatedDate WHERE UserTypeId=@UserTypeId", new { UserTypeId = userTypeId, UserTypeName = userTypeName, UserTypeDescription = userTypeDescription, UpdatedBy = updatedBy, UpdatedDate = GetRealDate() });

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