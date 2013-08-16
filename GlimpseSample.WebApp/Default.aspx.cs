using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Transactions;
using System.Web.Configuration;
using System.Web.UI;

namespace GlimpseSample.WebApp
{
    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateUsers();
            }
        }

        private void PopulateUsers()
        {
            string connectionStringName = WebConfigurationManager.AppSettings[Constants.AppSettings.ConnectionString];

            if (string.IsNullOrWhiteSpace(connectionStringName))
            {
                throw new ConfigurationErrorsException(string.Format("Missing value for application key: {0}", Constants.AppSettings.ConnectionString));
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

            DataTable dataTableUsers = new DataTable();

            using (TransactionScope transactionScope = new TransactionScope())
            {
                using (DbConnection connection = dbProviderFactory.CreateConnection())
                {
                    if (connection == null)
                    {
                        throw new InvalidOperationException("Unable to create connection");
                    }

                    connection.ConnectionString = connectionStringSettings.ConnectionString;

                    using (DbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Id, Name FROM Users ORDER BY Name ASC";
                        command.CommandTimeout = Convert.ToInt32(WebConfigurationManager.AppSettings[Constants.AppSettings.CommandTimeout]);
                        command.CommandType = CommandType.Text;

                        using (DbDataAdapter dataAdapter = dbProviderFactory.CreateDataAdapter())
                        {
                            if (dataAdapter == null)
                            {
                                throw new InvalidOperationException("Unable to create data adapter");
                            }

                            dataAdapter.SelectCommand = command;

                            connection.Open();
                            dataAdapter.Fill(dataTableUsers);
                        }
                    }

                    transactionScope.Complete();
                }
            }

            gridUsers.DataSource = dataTableUsers;
            gridUsers.DataBind();
        }
    }
}