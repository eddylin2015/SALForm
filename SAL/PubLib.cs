using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.Data.Common;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SAL
{
    public enum RoleDefs
    {
        err_user = -1, err_pass = 0, SysAdmin = 1, DataUpdate = 2,
        Marol = 3, ClassMaster = 4, Teacher = 5, Staff = 6,
        ActivityCenter = 7, SED = 8, SV = 9, SchoolDoctor = 10,
        SchoolAdmin = 11, SchoolAdminOffice = 12, KStaff = 13, KTeacher = 14
    };

    public class SystemPrivilegeMng
    {
        public SidebarItem sidebaritem;
        protected Form ParentForm;
        protected string subSysPrivilege;
        public int SystemID = 0;
        public MenuItem ss_mi;
        public SystemPrivilegeMng(Form Sender, string subSysPrivilege)
        {
            ParentForm = Sender;
            this.subSysPrivilege = subSysPrivilege;
        }
        ~SystemPrivilegeMng() {   }
        public virtual void AddMenu(MainMenu mm) {  }
        public void RemoveMenu(MainMenu mm)  { if (ss_mi != null)  mm.MenuItems.Remove(ss_mi);  }
    }
    public interface iFind_CNameDialog
    {
        string getCNAME();
        DialogResult ShowDialog();
        string ReturnStaf_ref_Set();
    }
    public class userinfo
    {
        public string username;
        public int userid;
        public int RoleID;
        public string SysPrivilege;
        public string SubSysPrivilege;
        public Hashtable FormPrivilegeSet;
        public Hashtable Dict = new Hashtable();
        public Form MainForm = null;
    }
    public interface iPub
    {
        int login(string usr, string pwd, String domain);
        void logout();
        DbConnection getDBConn();
        ESData getESD();
        void updateIniTempdata();
        string TempdataKey(string key);
    }
    public class Pub:iPub
    {
        protected static Pub _inst;
        private static string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
        public static string AppPath{ get { return appPath.Substring(6); } }
        protected virtual System.Data.Common.DbConnection _getdbconn() { return null; }
        protected string _connstr;
        public string dbname;
        public string getAppPath() { return appPath.Substring(6); }
        public List<SystemPrivilegeMng> listSubSys;
        public userinfo curr_userinfo;
        public string YM
        {
            get
            {
                String[] stra = dbname.Split('_');
                if(stra.Length>1 && stra[1].Length > 6)
                {
                    return stra[1].Substring(0, 6);
                }else
                {
                    return "000000";
                }
            }
        }
        public virtual ESData getESD() { return null; }
        public static Pub cfg { get { return _inst; } set { _inst = value; }  }
        public static Hashtable dict = new Hashtable();
        public static Hashtable tempdata = new Hashtable();
        public System.Data.Common.DbConnection dbconn { get { return _getdbconn(); } }
        public System.Data.Common.DbConnection getDBConn(){ return _getdbconn();    }
        public Pub()
        {
            String line;
            using (StreamReader srDict = new StreamReader("dictionary.dat", Encoding.Default))
                {
                    while ((line = srDict.ReadLine()) != null)
                    {
                        string[] astr = line.Split('=');
                        dict.Add(astr[0], astr[1]);
                    }
                    srDict.Close();
                }
                using (FileStream fs = new FileStream("tempdata.xml", FileMode.Open))
                {
                    XmlTextReader r = new XmlTextReader(fs);
                    string tempstr = null;
                    while (r.Read())
                    {
                        if (r.NodeType == XmlNodeType.Element)
                        {
                            if (r.Name == "tempdata") continue;
                            tempstr = r.Name;
                        }
                        else if (r.NodeType == XmlNodeType.Text)
                        {
                            tempdata.Add(tempstr, r.Value);
                        }
                    }
                    fs.Close();
                }
        }
        public void updateIniTempdata()
        {
            FileStream fs = new FileStream(this.getAppPath() + @"\tempdata.xml", FileMode.Create);
            XmlTextWriter w = new XmlTextWriter(fs, Encoding.UTF8);
            w.WriteStartDocument();
            w.WriteStartElement("tempdata");
            foreach (DictionaryEntry entry in Pub.tempdata)
            {
                w.WriteElementString(entry.Key.ToString(), entry.Value.ToString());
            }
            w.WriteEndElement();
            w.WriteEndDocument();
            w.Flush();
            w.Close();
            fs.Close();
            fs.Dispose();
        }
        public string TempdataKey(string key)
        {
            if (tempdata.ContainsKey(key)) return tempdata[key].ToString();
            tempdata.Add(key, ""); return "";
        }
        public virtual int login(string usr, string pwd, String domain)
        {
            return -1;
        }
        public void logout()
        {
            if (curr_userinfo != null)
            {
                curr_userinfo.FormPrivilegeSet.Clear();
                curr_userinfo.FormPrivilegeSet = null;
                curr_userinfo.Dict.Clear();
                curr_userinfo.Dict = null;
                curr_userinfo = null;
            }
            listSubSys = null;
        }
        protected static bool LDAPUserExists(string usr, String pwd)
        {
            LdapAuthentication ldap00 = new LdapAuthentication("LDAP://192.168.101.243");
            return (ldap00.IsAuthenticated("mbc", usr, pwd));
        }
        protected static String MYITUserExists(string UserName, String Pwd)
        {
            String args = String.Format("u={0}&p={1}", UserName, Pwd);
            bool post = true;
            string uri = "http://192.168.101.250/loginMyitstafref.php";
            return httpTools.httpGet_str(uri, args, post);
        }
    }

    public class odbc_pub : Pub
    {
        private string host;
        private string mysqlhost;
        private string ver;
        private string myodbcver;
        public odbc_pub(string configfile):base()
        {
            if (configfile != null)
            {
                StreamReader sr = new StreamReader(configfile, Encoding.Default);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] astr = line.Split('=');
                    if (astr[0] == "HOST") host = astr[1];
                    if (astr[0] == "MYSQLHOST") mysqlhost = astr[1];
                    if (astr[0] == "VER") ver = astr[1];
                    if (astr[0] == "MYODBCVER") myodbcver = astr[1];
                }
                using (StreamReader srDict = new StreamReader("dictionary.dat", Encoding.Default))
                {
                    while ((line = srDict.ReadLine()) != null)
                    {
                        string[] astr = line.Split('=');
                        dict.Add(astr[0], astr[1]);
                    }
                    srDict.Close();
                }
                using (FileStream fs = new FileStream("tempdata.xml", FileMode.Open))
                {
                    XmlTextReader r = new XmlTextReader(fs);
                    string tempstr = null;
                    while (r.Read())
                    {
                        if (r.NodeType == XmlNodeType.Element)
                        {
                            if (r.Name == "tempdata") continue;
                            tempstr = r.Name;
                        }
                        else if (r.NodeType == XmlNodeType.Text)
                        {
                            tempdata.Add(tempstr, r.Value);
                        }
                    }
                    fs.Close();
                }
            }
        }
    protected override System.Data.Common.DbConnection _getdbconn()
    {
            if (_dbconn != null && _dbconn.State != System.Data.ConnectionState.Open)
                _dbconn.Open();
            return _dbconn;
    }
    private OdbcConnection _dbconn;
}
    public class httpTools
    {
        public static string httpGet_str(string uri,string args,bool post)
        {
            string userAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.8.1.11) Gecko/20071127 Firefox/2.0.0.11"; // WINDOWS
            if (!post)
            {
                WebClient wc = new WebClient();
                wc.Headers[HttpRequestHeader.UserAgent] = userAgent;
                wc.Headers[HttpRequestHeader.Cookie] = "pass=deleted";
                try{
                    if (args == null)
                        return wc.DownloadString(uri);
                    else
                        return wc.DownloadString(uri + "?" + args);
                }catch (WebException){
                    return null;
                }
            }
            else
            {
                try
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
                    req.Method = WebRequestMethods.Http.Post;
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.UserAgent = userAgent;
                    byte[] bytes = Encoding.Default.GetBytes(args);
                    req.ContentLength = bytes.Length;
                    Stream reqStream = req.GetRequestStream();
                    reqStream.Write(bytes, 0, bytes.Length);
                    reqStream.Close();
                    WebResponse resp = req.GetResponse();
                    Stream newStream = resp.GetResponseStream();
                    StreamReader sr = new StreamReader(newStream);
                    string result = sr.ReadToEnd();
                    sr.Dispose();
                    newStream.Dispose();
                    return result;
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }
    }
    public class LdapAuthentication
    {
        private String _path;
        private String _filterAttribute;
        public LdapAuthentication(String path)
        {
            _path = path;
        }
        public bool IsAuthenticated(String domain, String username, String pwd)
        {
            String domainAndUsername = domain + @"\" + username;
            DirectoryEntry entry = new DirectoryEntry(_path, domainAndUsername, pwd);
            try
            {	//Bind to the native AdsObject to force authentication.			
                Object obj = entry.NativeObject;
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(SAMAccountName=" + username + ")";
                search.PropertiesToLoad.Add("cn");
                SearchResult result = search.FindOne();
                if (null == result)
                    return false;
                //Update the new path to the user in the directory.
                _path = result.Path;
                _filterAttribute = (String)result.Properties["cn"][0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error authenticating user. " + ex.Message);
                return false;
            }
            return true;
        }
        public String GetGroups()
        {
            DirectorySearcher search = new DirectorySearcher(_path);
            search.Filter = "(cn=" + _filterAttribute + ")";
            search.PropertiesToLoad.Add("memberOf");
            StringBuilder groupNames = new StringBuilder();
            try
            {
                SearchResult result = search.FindOne();
                int propertyCount = result.Properties["memberOf"].Count;
                String dn;
                int equalsIndex, commaIndex;
                for (int propertyCounter = 0; propertyCounter < propertyCount; propertyCounter++)
                {
                    dn = (String)result.Properties["memberOf"][propertyCounter];
                    equalsIndex = dn.IndexOf("=", 1);
                    commaIndex = dn.IndexOf(",", 1);
                    if (-1 == equalsIndex)
                    {
                        return null;
                    }
                    groupNames.Append(dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1));
                    groupNames.Append("|");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error obtaining group names. " + ex.Message);
            }
            return groupNames.ToString();
        }
    }
    /// <summary>
    /// 不用
    /// </summary>
    public class PubData
    {
        public static string RdrGetStr(OdbcDataReader rdr, int index)
        {
            try
            {
                if (rdr.IsDBNull(index))
                {
                    return "";
                }
                else
                {
                    return rdr.GetString(index);
                }
            }
            catch
            {
                return "";
            }
        }
        public static System.Data.DataSet getSQLDataSet(string sql, string tablename)
        {

            if (Pub.cfg.dbconn.State == System.Data.ConnectionState.Closed)
            {
                Pub.cfg.dbconn.Open();
            }

            System.Data.DataSet ds = new System.Data.DataSet();
            OdbcDataAdapter da = new OdbcDataAdapter(sql,(OdbcConnection) Pub.cfg.dbconn);
            da.TableMappings.Add("Table", tablename);
            da.Fill(ds);
            return ds;
        }
        public static System.Data.DataTable getSQLDataTable(string sql, string tablename)
        {

            if (Pub.cfg.dbconn.State == System.Data.ConnectionState.Closed)
            {
                Pub.cfg.dbconn.Open();
            }

            System.Data.DataSet ds = new System.Data.DataSet();
            OdbcDataAdapter da = new OdbcDataAdapter(sql, (OdbcConnection)Pub.cfg.dbconn);
            da.TableMappings.Add("Table", tablename);
            da.Fill(ds);
            return ds.Tables[tablename];
        }

    }
    
    /// <summary>
    /// 不用
    /// </summary>
    public class PubDataTable
    {
        private System.Data.DataTable dt;
        private OdbcDataAdapter da;
        public PubDataTable(string sql, string tablename)
        {

            if (Pub.cfg.dbconn.State == System.Data.ConnectionState.Closed)
            {
                Pub.cfg.dbconn.Open();
            }

            da = new OdbcDataAdapter(sql, (OdbcConnection)Pub.cfg.dbconn);
            OdbcCommandBuilder cmdbdr = new OdbcCommandBuilder(da);
            System.Data.DataSet ds = new System.Data.DataSet();
            da.TableMappings.Add("Table", tablename);
            da.Fill(ds);
            dt = ds.Tables[tablename];
        }
        public System.Data.DataTable getDataTable()
        {
            return dt;
        }
        public void update()
        {
            da.Update(dt);
        }
    }
    /// <summary>
    /// 不用
    /// </summary>
    public class PubDialog
    {
        public static void ShowUpdateCnt(int cnt) { MessageBox.Show("更新了" + cnt.ToString() + "筆資料"); }
    }
    public class PubRStudinfo
    {
        public string stuf_ref;
        public string dsej_ref;
        public string c_name;
        public string e_name;
        public string curr_class;
        public string curr_seat;
        public string sex;
        public string date_birth;
        public string tel;
        public string f_name;
        public string f_tel_offi;
        public string f_tel_home;
        public string m_name;
        public string m_tel_offi;
        public string m_tel_home;
        public string g_name;
        public string g_tel_offi;
        public string g_tel_home;
        public string em_name;
        public string em_tel;
    }
    public class PubUtils
    {
        public static int cCount(string s)
        {
            int count = 0;
            foreach (char c in s)
            {
                int i1 = c;
                if (i1 > 256)
                {
                    count++;
                }
            }
            return count;
        }
        public static string frmtStr(string s, int leterlong)
        {

            int i = leterlong - cCount(s);
            string frmtstr = "{0,-" + i.ToString() + "}";
            return string.Format(frmtstr, s);

        }
        public static OdbcType convertDataType(Type t)
        {
            if (t is String) { return OdbcType.VarChar; }
            else if (t is float) { return OdbcType.Real; }
            else if (t is double) { return OdbcType.Double; }
            else if (t is DateTime) { return OdbcType.DateTime; }
            return OdbcType.Char;
        }
        public static int AdjustAge(DateTime dt)
        {
            DateTime dn = DateTime.Now;
            int offset = 0;
            if (dn.Month >= dt.Month) offset = 1;
            int cage = dn.Year - dt.Year + offset - 1;
            return cage;
        }
        public static void TBInsertText(TextBox rt, string itext)
        {
            int startindex = rt.SelectionStart;
            rt.Text = rt.Text.Insert(rt.SelectionStart, itext);
            rt.Focus();
            rt.SelectionStart = startindex + itext.Length;
        }
        /// <summary>
        /// Tokenizer Token單詞列表
        /// </summary>
        /// <param name="mystring"></param>
        /// <param name="separators"></param>
        /// <returns></returns>
        /// <example><code>
        ///    string mystring = " i just wat the words,not the punctuation. and not the spaces, form this sentence";
        ///    foreach (string item in Tokenizer(mystring,new char[]{' ',','}))
        ///        Console.WriteLine(item);
        /// </code>
        /// </example>
        static List<string> Tokenizer(string mystring, char[] separators)
        {
            List<string> ls = new List<string>();
            if (separators == null) separators = new char[] { ' ', ',', '.', ':', ';', '?', '!' };
            int startPos = 0;
            int endPos = 0;
            do
            {
                endPos = mystring.IndexOfAny(separators, startPos);
                if (endPos == -1) endPos = mystring.Length;
                if (endPos != startPos)
                    ls.Add(mystring.Substring(startPos, (endPos - startPos)));
                startPos = (endPos + 1);
            } while (startPos < mystring.Length);
            return ls;
        }
    }
   

}
