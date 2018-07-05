using Newtonsoft.Json;
using PushApi.Common.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PushApi.Common.Repositories
{
    public class DbRepository
    {
        private string _connectionString;

        public DbRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["db"].ConnectionString;
        }

        public async Task<List<Platform>> GetPns(string appId, string userId)
        {
            var pns = new List<Platform>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = $"SELECT Platform FROM PushNotificationRegistrations WHERE AppId = '{appId}' AND UserId = '{userId}'";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    await conn.OpenAsync();
                    var reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        Platform pnsItem = (Platform)Enum.Parse(typeof(Platform), reader[0].ToString());

                        pns.Add(pnsItem);
                    }
                }
            }

            return pns;
        }

        private async Task<bool> RegistrationExists(string registrationId)
        {
            bool exists = false;

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = $"SELECT RegistrationId FROM PushNotificationRegistrations WHERE RegistrationId = '{registrationId}'";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    await conn.OpenAsync();
                    var reader = await cmd.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        exists = true;
                    }
                }
            }

            return exists;
        }

        public async Task<List<string>> GetRegistrationExcept(PushRegistration pushRegistration)
        {
            var registrations = new List<string>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = $"SELECT RegistrationId FROM PushNotificationRegistrations WHERE RegistrationId <> '{pushRegistration.RegistrationId}' AND AppId = '{pushRegistration.AppId}' AND UserId = '{pushRegistration.UserId}'";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    await conn.OpenAsync();
                    var reader = await cmd.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            registrations.Add(reader[0].ToString());
                        }
                    }
                }
            }

            return registrations;
        }

        public async Task<bool> Insert(PushRegistration pushRegistration)
        {
            var success = false;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string sql = $"INSERT INTO PushNotificationRegistrations (RegistrationId, AppId, UserId, DeviceToken, Platform, Tags) VALUES ('{pushRegistration.RegistrationId}', '{pushRegistration.AppId}', '{pushRegistration.UserId}', '{pushRegistration.DeviceToken}', {(int)pushRegistration.Platform}, '{JsonConvert.SerializeObject(pushRegistration.Tags)}')";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    await cmd.ExecuteScalarAsync();
                    success = true;
                }
            }

            return success;
        }

        public async Task<bool> Update(PushRegistration pushRegistration)
        {
            var success = false;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string sql = $"UPDATE PushNotificationRegistrations SET AppId = '{pushRegistration.AppId}', UserId = '{pushRegistration.UserId}', DeviceToken = '{pushRegistration.DeviceToken}', Platform = {(int)pushRegistration.Platform}, Tags = '{JsonConvert.SerializeObject(pushRegistration.Tags)}' WHERE RegistrationId = '{pushRegistration.RegistrationId}'";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    await cmd.ExecuteScalarAsync();
                    success = true;
                }
            }

            return success;
        }

        public async Task<bool> Upsert(PushRegistration pushRegistration)
        {
            var success = false;

            var exists = await RegistrationExists(pushRegistration.RegistrationId);

            if (exists)
            {
                success = await Update(pushRegistration);
            }
            else
            {
                success = await Insert(pushRegistration);
            }

            return success;
        }

        public async Task<bool> Delete(string registrationId)
        {
            var success = false;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string sql = $"DELETE FROM PushNotificationRegistrations WHERE RegistrationId = '{registrationId}'";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    await cmd.ExecuteScalarAsync();
                    success = true;
                }
            }

            return success;
        }

        public async Task<AzureNotificationHub> GetAzureNotificationHubEndpoint(string appId)
        {
            AzureNotificationHub hub = new AzureNotificationHub();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = $"SELECT AppId, Endpoint, HubName FROM AzureNotificationHubs WHERE AppId = '{appId}'";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    await conn.OpenAsync();
                    var reader = await cmd.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            hub.AppId = appId;
                            hub.Endpoint = reader[1].ToString();
                            hub.HubName = reader[2].ToString();
                          
                        }
                    }
                }
            }

            return hub;
        }

    }
}