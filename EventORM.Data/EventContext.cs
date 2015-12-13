using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace EventORM.Data
{
    public class EventContext
    {
        public string ContextName { get; private set; }
        public string ConnectionString { get; private set; }

        public EventContext()
        {
            ContextName = GetType().Name;
            ConnectionString = ConfigurationManager.ConnectionStrings[ContextName].ConnectionString;
            Setup();
        }

        public EventContext(string connStrName)
        {
            ContextName = connStrName;
            ConnectionString = ConfigurationManager.ConnectionStrings[ContextName].ConnectionString;
            Setup();
        }

        private void Setup()
        {
            var items = GetType()
                 .GetProperties()
                 .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(RecordSet<>));

            foreach (var type in items)
            {
                var d1 = typeof(RecordSet<>);
                var d2 = type.PropertyType.GenericTypeArguments[0];
                Type[] typeArgs = { d2 };
                var rs = d1.MakeGenericType(typeArgs);
                var o = Activator.CreateInstance(rs);
                var p1 = rs.GetField("_connStr", BindingFlags.Instance | BindingFlags.NonPublic);
                var p2 = rs.GetField("_contextName", BindingFlags.Instance | BindingFlags.NonPublic);
                p1.SetValue(o, ConnectionString);
                p2.SetValue(o, ContextName);
                type.SetValue(this, o, null);
            }
        }

        public void SetupDatabase()
        {
            var items = GetType()
                 .GetProperties()
                 .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(RecordSet<>));

            var tableProperties = new Dictionary<string, PropertyInfo>();

            foreach (var type in items)
            {
                var generic = type.PropertyType.GenericTypeArguments[0];
                foreach (var prop in generic.GetProperties())
                {
                    if (prop.GetMethod.IsVirtual)
                        continue;

                    tableProperties.Add(String.Format("{0}_{1}", generic.Name, prop.Name), prop);
                }
            }

            var tableName = GetType().Name;

            string createTableCmdStr = String.Format("CREATE TABLE [dbo].[{0}] (", tableName);

            foreach (var item in tableProperties)
            {
                createTableCmdStr += String.Format("[{0}] {1},",
                    item.Key,
                    GetDbType(item.Value.PropertyType));
            }

            createTableCmdStr += String.Format("CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED (", tableName);

            foreach (var item in tableProperties)
            {
                var key = item.Key.Split('_')[0];
                var field = item.Key.Split('_')[1];
                if (!field.StartsWith(key))
                    continue;

                createTableCmdStr += String.Format("[{0}],", item.Key);
            }

            createTableCmdStr = createTableCmdStr.TrimEnd(',');

            createTableCmdStr += ") WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]";
            createTableCmdStr += ") ON [PRIMARY]";

            using (var cmd = new SqlCommand(createTableCmdStr, new SqlConnection(ConnectionString)))
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }

            tableName = String.Format("{0}_Events", tableName);
            createTableCmdStr = String.Format("CREATE TABLE [dbo].[{0}] (", tableName);
            createTableCmdStr += "[EventId] [uniqueidentifier] NOT NULL,";
            createTableCmdStr += "[DateTime] [datetime2](7) NOT NULL,";
            createTableCmdStr += "[Details] [nvarchar](max) NOT NULL,";
            createTableCmdStr += String.Format("CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED ", tableName);
            createTableCmdStr += "([EventId] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";

            using (var cmd = new SqlCommand(createTableCmdStr, new SqlConnection(ConnectionString)))
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }

            createTableCmdStr = String.Format("ALTER TABLE [dbo].[{0}] ADD  CONSTRAINT [DF_{0}_DateTime]  DEFAULT (getdate()) FOR [DateTime]", tableName);
            using (var cmd = new SqlCommand(createTableCmdStr, new SqlConnection(ConnectionString)))
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
        }


        private string GetDbType(Type clrType)
        {
            if (clrType == typeof(int))
                return "[int] DEFAULT 0";
            else if (clrType == typeof(string))
                return "[nvarchar](max)";

            throw new NotImplementedException();
        }
    }
}
