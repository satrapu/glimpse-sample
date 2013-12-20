using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GlimpseSample.WebApp
{
    public class AdoNet : Page
    {
        private const int USERS_AMOUNT = 25;
        protected GridView gridUsers;

        protected void Page_Load(object sender, EventArgs e)
        {
            PopulateUsers();
        }

        private void PopulateUsers()
        {
            string connectionStringName = WebConfigurationManager.AppSettings[Constants.AppSettings.CONNECTION_STRING];

            if (string.IsNullOrWhiteSpace(connectionStringName))
            {
                throw new ConfigurationErrorsException(string.Format("Missing value for application key: {0}", Constants.AppSettings.CONNECTION_STRING));
            }

            ConnectionStringSettings connectionStringSettings = WebConfigurationManager.ConnectionStrings[connectionStringName];

            if (string.IsNullOrWhiteSpace(connectionStringSettings.ProviderName))
            {
                throw new ConfigurationErrorsException(
                        string.Format("The connection string identified by name {0} does not define a provider name for a DbProviderFactory", connectionStringName));
            }

            DbProviderFactory dbProviderFactory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);

            if (dbProviderFactory == null)
            {
                throw new ConfigurationErrorsException(string.Format("Unable to instantiate A DbProviderFactory using provider name: {0}",
                        connectionStringSettings.ProviderName));
            }

            var dataTableUsers = new DataTable();

            using (var transactionScope = new TransactionScope())
            {
                using (DbConnection connection = dbProviderFactory.CreateConnection())
                {
                    Debug.Assert(connection != null, "connection != null");
                    connection.ConnectionString = connectionStringSettings.ConnectionString;
                    connection.Open();

                    using (DbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "CREATE TABLE Users(Id INT PRIMARY KEY, Name VARCHAR NOT NULL)";
                        command.CommandTimeout = Convert.ToInt32(WebConfigurationManager.AppSettings[Constants.AppSettings.COMMAND_TIMEOUT]);
                        command.CommandType = CommandType.Text;

                        command.ExecuteNonQuery();
                    }

                    using (DbCommand command = connection.CreateCommand())
                    {
                        DbParameter dbParameterId = command.CreateParameter();
                        dbParameterId.ParameterName = "@Id";

                        DbParameter dbParameterName = command.CreateParameter();
                        dbParameterName.ParameterName = "@Name";

                        command.CommandText = "INSERT INTO Users(Id, Name) VALUES (@Id, @Name)";
                        command.CommandTimeout = Convert.ToInt32(WebConfigurationManager.AppSettings[Constants.AppSettings.COMMAND_TIMEOUT]);
                        command.CommandType = CommandType.Text;
                        command.Parameters.Add(dbParameterId);
                        command.Parameters.Add(dbParameterName);

                        foreach (var index in Enumerable.Range(1, USERS_AMOUNT))
                        {
                            dbParameterId.Value = index;
                            dbParameterName.Value = Guid.NewGuid().ToString("N");
                            command.ExecuteNonQuery();
                        }
                    }

                    using (DbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Id, Name FROM Users ORDER BY Id ASC";
                        command.CommandTimeout = Convert.ToInt32(WebConfigurationManager.AppSettings[Constants.AppSettings.COMMAND_TIMEOUT]);
                        command.CommandType = CommandType.Text;

                        using (DbDataAdapter dataAdapter = dbProviderFactory.CreateDataAdapter())
                        {
                            Debug.Assert(dataAdapter != null, "dataAdapter != null");
                            dataAdapter.SelectCommand = command;
                            dataAdapter.Fill(dataTableUsers);
                        }
                    }
                }

                transactionScope.Complete();
            }

            gridUsers.DataSource = dataTableUsers;
            gridUsers.DataBind();
        }
    }
}