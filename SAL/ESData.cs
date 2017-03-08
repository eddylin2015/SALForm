using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAL
{
        interface iESData
        {
            void Open_Conn();
            void Close_Conn();
            DbDataReader Reader(string sql);
            DbDataReader Reader_ShowTables();
            int Exec(String sql);
            String ViewTableTxt(String TableName);
            DbCommand GetCommand(String cmdsql);
            DbCommandBuilder GetDbCommandBuilder(DbDataAdapter da);
            SQLiteCommand GetSQLiteCommand(String cmdsql, SQLiteParameter[] paras);
            String ShowTablesSQL();
        }
        public class ESData : iESData
        {
        public ESData()
        {

        }
        protected String DBNAME =null;
        protected String _conn_txt = null;

        private static ESData _instance = null;
            public Hashtable dict = new Hashtable();
            public static ESData SetDB(String dbname)
            {
            if (_instance == null) { _instance = new SQLite_ESData(dbname); } else { _instance.SetDBName(dbname); }
            return _instance;
            }
            protected virtual void SetDBName(String dbname)
            {
            }
            public static ESData GetInst
            {
                get
                {
                    return _instance;
                }
            }
            protected static System.Data.Common.DbConnection _conn = null;
            protected virtual String GetConn_Txt() { return null; }
            public String Conn_Txt { get { return GetConn_Txt(); } }
            protected virtual DbConnection GetConn() { return null; }
            public DbConnection conn
            {
                get {return GetConn(); }
            }
            public void Open_Conn()
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
            }
            public void Close_Conn()
            {
                _close_conn();
            }
            protected virtual void _close_conn() { }
            public virtual System.Data.Common.DbDataReader Reader(string sql) { return null; }
            public virtual System.Data.Common.DbDataReader Reader_ShowTables() { return null; }
            public virtual int Exec(String sql)  {return 0; }
            public virtual DbCommand GetCommand(String cmdsql) { return null; }
            public virtual DbCommandBuilder GetDbCommandBuilder(DbDataAdapter da) { return null; }
            public virtual DbDataAdapter GetAdapter() { return null; }
            public virtual DbDataAdapter GetAdapter(DbCommand cmd) { return null; }
            public virtual SQLiteCommand GetSQLiteCommand(String cmdsqsl, SQLiteParameter[] paras) { return null; }
            public virtual String ShowTablesSQL() { return null; }
            public String ViewTableTxt(String TableName)
            {
                ESData.GetInst.Open_Conn();
                String txt = "TabeName:" + TableName + "\n";
                System.Data.Common.DbDataReader dr = ESData.GetInst.Reader(String.Format("select * from {0};", TableName));
                for (int i = 0; i < dr.FieldCount; i++)  txt += dr.GetName(i) + "\t ";
                txt += "\n";//if (dr.HasRows)
                while (dr.Read()) { 
                     for (int i = 0; i < dr.FieldCount; i++)
                            try{ txt += dr[i].ToString() + "\t "; }
                     catch{MessageBox.Show(dr[0].ToString());}
                     txt += "\n";
                }
                dr.Close();
                dr.Dispose();
                return txt;
            }
        }
    public class SQLite_ESData : ESData
    {
        public SQLite_ESData(String dbname):base()
        {
            SetDBName(dbname);
        }
        protected override void SetDBName(string dbname)
        {
            DBNAME = dbname ;
            _conn_txt = string.Format("Data Source=\"{0}\\{1}\"", Pub.AppPath, DBNAME);
        }
        private static SQLiteConnection sqliteconn = null;
        protected override string GetConn_Txt()
        {
            return _conn_txt;
        }
        protected override DbConnection GetConn()
        {
            if (_conn == null)
            {
                sqliteconn = new SQLiteConnection(_conn_txt);
                _conn = sqliteconn;
            }
            return _conn;
        }
        protected override void _close_conn()
        {
            if (_conn != null)
            {
                _conn.Close();
                _conn.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                SQLiteConnection.ClearAllPools();
                _conn = null;
            }
        }
        public override DbDataReader Reader(string sql)
        {
            Open_Conn();
            SQLiteCommand cmd = new SQLiteCommand(sql, sqliteconn);
            return cmd.ExecuteReader();
        }
        public override DbDataReader Reader_ShowTables()
        {
            Open_Conn();
            SQLiteCommand cmd = new SQLiteCommand("select tbl_name from sqlite_master where type='table' order by tbl_name;", sqliteconn);
            return cmd.ExecuteReader();
        }
        public override int Exec(string sql)
        {
            Open_Conn();
            return (new SQLiteCommand(sql, sqliteconn)).ExecuteNonQuery();
        }
        public override DbCommand GetCommand(string cmdsql)
        {
            Open_Conn();
            return new SQLiteCommand(cmdsql, sqliteconn);
        }
        public override DbCommandBuilder GetDbCommandBuilder(DbDataAdapter da)
        {
            Open_Conn();
            if (da is SQLiteDataAdapter)
            {
                return new SQLiteCommandBuilder((SQLiteDataAdapter)da);
            }
            else
            {
                return base.GetDbCommandBuilder(da);
            }
        }
        public override DbDataAdapter GetAdapter()
        {
            Open_Conn();
            return new SQLiteDataAdapter();
        }
        public override DbDataAdapter GetAdapter(DbCommand cmd)
        {
            Open_Conn();
            if (cmd is SQLiteCommand)
            {
                return new SQLiteDataAdapter((SQLiteCommand)cmd);
            }
            else { return base.GetAdapter(cmd); }
        }
        public override SQLiteCommand GetSQLiteCommand(string cmdsql, SQLiteParameter[] paras)
        {
            Open_Conn();
            SQLiteCommand cmd = new SQLiteCommand(cmdsql, sqliteconn);
            if (paras != null)
                for (int i = 0; i < paras.Length; i++)
                    cmd.Parameters.Add(paras[i]);
            return cmd;
        }
        public override string ShowTablesSQL()
        {
            return "select tbl_name from sqlite_master where type='table' order by tbl_name;";
        }
    }
}
