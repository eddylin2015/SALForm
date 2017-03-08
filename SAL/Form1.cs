using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.DirectoryServices.Protocols;
using System.Net;
using System.DirectoryServices;
using System.Security.Principal;
using System.Web.Security;
using System.Collections;
using System.Data.Odbc;
using System.Data.Common;
using System.IO;
using System.Data.SQLite;

namespace SAL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            mainform = this;
            String dbname=AppSettingsConfigure.Read("dbname");
            message_center.tssl_msg("請登MBC網域");
            message_center.tssl_dbname(dbname);
            Pub.cfg = new es_admin_pub("config.ini",dbname);
            this.Menu = new MainMenu();
            this.IsMdiContainer = true;
            this.Text =string.Format("Title:【SAL】");
            sidebar.Visible = false;
            this.Menu.MenuItems.Add( new MenuItem("三", sidebar_click) ); 
            MenuItem mi_file = this.Menu.MenuItems.Add("A.系統");
            mi_file.MenuItems.Add(new MenuItem("登入", login_click ));
            mi_file.MenuItems.Add(new MenuItem("登出", msyslogout_click));
            mi_file.MenuItems.Add(new MenuItem("EXIT", msysexit_click));
        }
        public static Form mainform;
        private void sidebar_click(Object sender, EventArgs e)
        {
            if (Pub.cfg.curr_userinfo != null)
            {
                sidebar.Visible = !sidebar.Visible;
            }
        }
        private void login_click(Object sender, EventArgs e)
        {
            if (Pub.cfg.curr_userinfo == null)
            {
                LoginUserPwd lup = new SAL.LoginUserPwd();
                if (lup.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("");
                    Pub.cfg.curr_userinfo.MainForm = this;
                    string[] SubSysPrivilegeArr = Pub.cfg.curr_userinfo.SubSysPrivilege.Split(';');
                    List<SidebarItem> sidebarItems = new List<SidebarItem>();
                    for (int i = 0; i < Pub.cfg.curr_userinfo.SysPrivilege.Length; i++)
                    {
                        if (Pub.cfg.curr_userinfo.SysPrivilege[i] == '1')
                        {
                            SystemPrivilegeMng instance = null;
                            if (i >= SubSysPrivilegeArr.Length)
                            {
                                instance = SystemPrivilegeMngDef.GetInstance(i, this, "");
                            }
                            else
                            {
                                instance = SystemPrivilegeMngDef.GetInstance(i, this, SubSysPrivilegeArr[i]);
                            }
                            if (instance != null)
                            {
                                Pub.cfg.listSubSys.Add(instance);
                                instance.AddMenu(this.Menu);
                                if (instance.sidebaritem != null)
                                    sidebarItems.Add(instance.sidebaritem);
                            }
                        }
                    }
                    Sidebar<Form1> sidebar_binding = new Sidebar<Form1>(this, sidebar, listView1, imageList1, sidebarItems);
                }
            }
        }
        private void msyslogout_click(Object sender, EventArgs e)
        {
            sidebar.Visible = false;
            if (MessageBox.Show("是否确定退出！", "登出",
                     MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK && Pub.cfg.curr_userinfo != null)
            {
                if (this.MdiChildren.Length > 0)
                {
                    MessageBox.Show("請先關閉所窗口!", "登出",
                     MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Pub.cfg.curr_userinfo = null;
                for (int i = 0; i < Pub.cfg.listSubSys.Count; i++)
                {
                    SystemPrivilegeMng instance = Pub.cfg.listSubSys[i];
                    instance.RemoveMenu(this.Menu);
                    instance = null;
                }
                Pub.cfg.listSubSys.Clear();
                Pub.cfg.listSubSys = null;
                Sidebar<Form1>.CleanSibebar(sidebar, listView1);
            }
        }
        private void msysexit_click(Object sender, EventArgs e)
        {
            this.Close();
        }
        private void mnuIcons_click(Object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.ArrangeIcons);
        }
        private void mnuCascade_click(Object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.Cascade);
        }
        private void mnuTileHorizontal_click(Object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileHorizontal);
        }
        private void mnuTileVertical_click(Object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileVertical);
        }
        private void CloseAllSubForm_click(Object sender, EventArgs e)
        {
            for (int i = MdiChildren.Length - 1; i > -1; i--)
                this.MdiChildren[i].Close();
        }
    }
    public class LoginUserPwd : Form
    {
        private Label usrlbl = new Label();
        private Label pwdlbl = new Label();
        private Label errorlbl = new Label();
        private Label domainlbl = new Label();
        private TextBox usr = new TextBox();
        private TextBox pwd = new TextBox();
        private Button loginbtn = new Button();
        private TextBox domain = new TextBox();
        private TableLayoutPanel tp = new TableLayoutPanel();

        public LoginUserPwd()
        {
            tp.RowCount = 5;
            tp.ColumnCount = 2;
            tp.Controls.Add(usrlbl, 0, 0);
            tp.Controls.Add(usr, 1, 0);
            tp.Controls.Add(pwdlbl, 0, 1);
            pwd.PasswordChar = '*';
            tp.Controls.Add(pwd, 1, 1);
            tp.Controls.Add(loginbtn, 1, 2);
            tp.Controls.Add(domainlbl, 0, 3);
            tp.Controls.Add(domain, 1, 3);
            tp.Controls.Add(errorlbl, 0, 4);
            tp.Dock = DockStyle.Fill;
            usrlbl.Text = "USER";
            pwdlbl.Text = "PWD";
            loginbtn.Text = "Login";
            domainlbl.Text = "網域";
            domain.Text = "mbc";
            errorlbl.Height = 40;
            errorlbl.Text = "Note:";

            usr.KeyUp += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter && usr.Text.Length>=2)
                    pwd.Focus();
            };
            pwd.KeyUp += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                    loginbtn.Focus();
            };
            loginbtn.Click += (sender,e)=>{
                if (Pub.cfg.login(usr.Text, pwd.Text, domain.Text)>0)
                {

                    errorlbl.Text = "login succ!";
                    message_center.tssl_msg("login succ!");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    errorlbl.Text += "login error!";
                    message_center.tssl_msg("login fail!");
                }
               
              
            };
            this.Controls.Add(tp);
            usr.Focus();
        }
    }
    public class message_center
    {
        public static void  tssl_msg(string msg)
        {
            Form1 mf= (Form1)Form1.mainform;
            mf.tssl_msg.Text = msg;
        }
        public static void tssl_dbname(string msg)
        {
            Form1 mf = (Form1)Form1.mainform;
            mf.tsldbname.Text = msg;
        }
    }
    public class es_admin_pub : Pub
    {
        public es_admin_pub(string configfile,string _dbname)
            : base()
        {
            dbname = _dbname;
            getESD();
        }
        public override  ESData getESD()
        {
            return ESData.SetDB(dbname);
        }
        protected override DbConnection _getdbconn()
        {
            //return base._getdbconn();
            if (dbname.Split('_').Length < 1) { forms.utls.alert("db_name error!"); }
            String fileName =this.getAppPath() + @"\" + dbname;
            FileInfo finfo = new FileInfo(fileName);
            _connstr = "Data Source=" + fileName;
            if (!finfo.Exists)
            {
                System.Data.SQLite.SQLiteConnection.CreateFile(fileName);
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK) {
                    string sql=File.ReadAllText(ofd.FileName);
                    using (SQLiteConnection conn= new System.Data.SQLite.SQLiteConnection(_connstr))
                    {
                        conn.Open();
                        using(SQLiteCommand cmd=new SQLiteCommand(sql, conn))
                        {
                            MessageBox.Show(String.Format("Create DB {0}", cmd.ExecuteNonQuery()));
                        }
                        
                        using (SQLiteCommand cmd = new SQLiteCommand("insert into sl_ctrldata(curr_ym)values('"+dbname.Split('_')[1]+"')", conn))
                        {
                            MessageBox.Show(String.Format("Create DB {0}", cmd.ExecuteNonQuery()));
                        }
                        conn.Close();
                        
                    }
                }
            }
            return new System.Data.SQLite.SQLiteConnection(_connstr);
        }
        /*#if DEBUG
                        MessageBox.Show("debug version");
        #else
                        String temp_s = es_lib.Publib.ConnConfigure.GetConnectionStringByName("MBC_TEMP_Name");
                        string msg = es_lib.Publib.ConnConfigure.ToggleConfigEncryption("sal.exe");
                        if (temp_s != null)
                        {
                            temp_s = temp_s.Split(':')[4];
                            int tempaa = int.Parse(temp_s.Substring(0, 1));
                            string tempbb = temp_s.Substring(tempaa, tempaa);
                            return (es_dblib.esdb.GetInstance(mysqlhost, "es_hrms", "hrms", tempbb + "bc")).GetConn();
                        }
                        else
                        {
                            throw new Exception("Conn STR ERR");
                        }
        #endif
                        */
        public override int login(string usr, string pwd, String domain)
        {
            //(usr.Equals("2002024") || usr.Equals("2006001")) &&
           // if ( Pub.LDAPUserExists(usr, pwd))
            if(true)
			{
                curr_userinfo = new userinfo();
                curr_userinfo.userid = 1;
                curr_userinfo.username = usr;
                string tempFormPrivilege = "1=1111111;2=001";
                string[] ls = tempFormPrivilege.Split(';');
                curr_userinfo.FormPrivilegeSet = new Hashtable();
                foreach (string s in ls)
                {
                    string[] ts = s.Split('=');
                    try
                    {
                        curr_userinfo.FormPrivilegeSet.Add(ts[0], ts[1]);
                    }
                    catch
                    {
                       // MessageBox.Show("From Privilege Error");
                    }
                }
                curr_userinfo.RoleID = 1;
                curr_userinfo.SysPrivilege = "11111111";
                curr_userinfo.SubSysPrivilege = "11111;11111111111111111111;1111111111111";
                listSubSys = new List<SystemPrivilegeMng>();
                return (int)RoleDefs.SchoolAdmin;
            }else
            {
                return (int)RoleDefs.err_user;
            }
        }
    }
   
}
