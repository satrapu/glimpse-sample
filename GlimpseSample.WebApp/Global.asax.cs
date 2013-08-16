using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Configuration;

namespace GlimpseSample.WebApp
{
    public class Global : HttpApplication
    {
        public Global()
        {
            Error += OnError;
            InitData();
        }

        private static void OnError(object sender, EventArgs e)
        {
        }

        private static void InitData()
        {
            string connectionStringName = WebConfigurationManager.AppSettings[Constants.AppSettings.ConnectionString];

            if (string.IsNullOrWhiteSpace(connectionStringName))
            {
                throw new TypeInitializationException(typeof (Global).AssemblyQualifiedName,
                        new ConfigurationErrorsException(string.Format("Missing value for application key: {0}", Constants.AppSettings.ConnectionString)));
            }

            ConnectionStringSettings connectionStringSettings = WebConfigurationManager.ConnectionStrings[connectionStringName];

            if (string.IsNullOrWhiteSpace(connectionStringSettings.ProviderName))
            {
                throw new TypeInitializationException(typeof (Global).AssemblyQualifiedName,
                        new ConfigurationErrorsException(
                                string.Format("The connection string identified by name {0} does not define a provider name for a DbProviderFactory", connectionStringName)));
            }

            DbProviderFactory dbProviderFactory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);

            if (dbProviderFactory == null)
            {
                throw new TypeInitializationException(typeof (Global).AssemblyQualifiedName,
                        new ConfigurationErrorsException(string.Format("Unable to instantiate A DbProviderFactory using provider name: {0}",
                                connectionStringSettings.ProviderName)));
            }

            using (TransactionScope transactionScope = new TransactionScope())
            {
                using (DbConnection connection = dbProviderFactory.CreateConnection())
                {
                    if (connection == null)
                    {
                        throw new TypeInitializationException(typeof (Global).AssemblyQualifiedName, new InvalidOperationException("Unable to create connection"));
                    }

                    connection.ConnectionString = connectionStringSettings.ConnectionString;

                    using (DbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "DROP TABLE IF EXISTS Users";
                        command.CommandTimeout = Convert.ToInt32(WebConfigurationManager.AppSettings[Constants.AppSettings.CommandTimeout]);
                        command.CommandType = CommandType.Text;

                        connection.Open();
                        command.ExecuteNonQuery();
                    }

                    using (DbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "CREATE TABLE Users(Id VARCHAR PRIMARY KEY ASC, Name VARCHAR NOT NULL)";
                        command.CommandTimeout = Convert.ToInt32(WebConfigurationManager.AppSettings[Constants.AppSettings.CommandTimeout]);
                        command.CommandType = CommandType.Text;

                        command.ExecuteNonQuery();
                    }

                    transactionScope.Complete();
                }
            }

            using (TransactionScope transactionScope = new TransactionScope())
            {
                using (DbConnection connection = dbProviderFactory.CreateConnection())
                {
                    if (connection == null)
                    {
                        throw new TypeInitializationException(typeof (Global).AssemblyQualifiedName, new InvalidOperationException("Unable to create connection"));
                    }

                    connection.ConnectionString = connectionStringSettings.ConnectionString;
                    connection.Open();

                    using (DbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO Users(Id, Name) VALUES (@Id, @Name)";
                        command.CommandTimeout = Convert.ToInt32(WebConfigurationManager.AppSettings[Constants.AppSettings.CommandTimeout]);
                        command.CommandType = CommandType.Text;

                        foreach (var index in Enumerable.Range(1, 100))
                        {
                            command.Parameters.Clear();

                            DbParameter dbParameterId = command.CreateParameter();
                            dbParameterId.ParameterName = "@Id";
                            dbParameterId.Value = Guid.NewGuid().ToString("N");

                            DbParameter dbParameterName = command.CreateParameter();
                            dbParameterName.ParameterName = "@Name";
                            dbParameterName.Value = "User#" + index;

                            command.Parameters.Add(dbParameterId);
                            command.Parameters.Add(dbParameterName);
                            command.ExecuteNonQuery();
                        }
                    }

                    transactionScope.Complete();
                }
            }
        }
    }
}