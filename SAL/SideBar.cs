using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAL
{
    public struct SidebarItem_Struct
    {
        public string title;
        public string fun;
        public int icontab;
        public SidebarItem_Struct(string t, string f, int icon)
        {
            title = t;
            fun = f;
            icontab = icon;
        }

    }
    public class SidebarItem
    {
        public string title;
        public int tabindex;
        public object t;
        public Type atype;
        public List<SidebarItem_Struct> subitems;
        public List<SidebarItem> next;
    }
    public class Sidebar<T>
    {
        private List<SidebarItem> sitems = null;
        public static void CleanSibebar(Panel _Panel, ListView _ListView)
        {
            _ListView.Clear();
            List<Control> lb = new List<Control>();
            foreach (Control ctl in _Panel.Controls)
            {
                if (ctl is Button)
                {
                    lb.Add(ctl);

                }
            }
            foreach (Control c in lb)
                _Panel.Controls.Remove(c);
        }
        public void IteratorSidebarItem(List<Button> li, List<SidebarItem> items, ref int tabcnt)
        {
            foreach (SidebarItem si in items)
            {
                Button btn = new Button();
                btn.Text = si.title;
                btn.TabIndex = tabcnt;
                si.tabindex = tabcnt;
                tabcnt++;
                btn.Click += ButtonClick;
                li.Add(btn);
                if (si.next != null)
                {
                    IteratorSidebarItem(li, si.next, ref tabcnt);
                }
            }
        }
        public SidebarItem_Struct IteratorSelectedSidebarItem_Struct(String SelectedItemText, List<SidebarItem> sitems, ref SidebarItem curr_SI)
        {
            foreach (SidebarItem si in sitems)
            {
                if (si.subitems != null)
                {
                    foreach (SidebarItem_Struct sis in si.subitems)
                    {
                        if (sis.title.Equals(SelectedItemText))
                        {
                            curr_SI = si;
                            return sis;   //Type t = Type.GetType(si.typestring);
                        }
                    }
                }
                if (si.next != null)
                {
                    SidebarItem_Struct ret = IteratorSelectedSidebarItem_Struct(SelectedItemText, si.next, ref curr_SI);
                    if (ret.icontab > -1) { return ret; }
                }
            }
            return new SidebarItem_Struct(null, null, -1);
        }
        public Sidebar(T _Parent, Panel _Panel, ListView _ListView, ImageList _imagelist, List<SidebarItem> _sitems)
        {
            parent = _Parent;
            imglist = _imagelist;
            sidebar = _Panel;
            listView1 = _ListView;
            sitems = _sitems;
            int tabcnt = 0;
            List<Button> temp_li = new List<Button>();
            IteratorSidebarItem(temp_li, sitems, ref tabcnt);
            for (int i = temp_li.Count - 1; i > -1; i--)
            {
                temp_li[i].Dock = DockStyle.Top;
                sidebar.Controls.Add(temp_li[i]);
            }
            listView1.SelectedIndexChanged += (sender, e) =>
            {
                if (listView1.SelectedIndices.Count > 0)
                {
                    SidebarItem si = null;
                    SidebarItem_Struct sis = IteratorSelectedSidebarItem_Struct(listView1.SelectedItems[0].Text, sitems, ref si);
                    if (sis.icontab > -1)
                    {
                        Type t = si.atype;
                        System.Reflection.MethodInfo mi = t.GetMethod(sis.fun);
                        if (mi != null)
                        {
                            object[] args = new object[2];
                            args[0] = this;
                            args[1] = new EventArgs();
                            mi.Invoke(si.t, args);
                        }
                        else { MessageBox.Show("未登記!"); }
                    }
                }
            };
        }
        private T parent;
        private ImageList imglist;
        private Panel sidebar;
        private ListView listView1;
        #region Handle events
        void IteratorIcon(ListView lv, List<SidebarItem> sitems, int clickedButtonTabIndex)
        {
            if (sitems != null)
            {
                foreach (SidebarItem si in sitems)
                {
                    if (si.tabindex == clickedButtonTabIndex && si.subitems != null)
                    {
                        foreach (SidebarItem_Struct sis in si.subitems)
                        {
                            listView1.Items.Add(sis.title, sis.icontab);
                        }
                        return;
                    }
                    IteratorIcon(lv, si.next, clickedButtonTabIndex);
                }
            }
        }
        void ButtonClick(object sender, System.EventArgs e)
        {
            // Get the clicked button...
            Button clickedButton = (Button)sender;
            // ... and it's tabindex
            int clickedButtonTabIndex = clickedButton.TabIndex;

            // Send each button to top or bottom as appropriate

            foreach (Control ctl in sidebar.Controls)
            {

                if (ctl is Button)
                {
                    Button btn = (Button)ctl;
                    if (btn.TabIndex > clickedButtonTabIndex)
                    {
                        if (btn.Dock != DockStyle.Bottom)
                        {
                            btn.Dock = DockStyle.Bottom;
                            // This is vital to preserve the correct order
                            btn.BringToFront();
                        }
                    }
                    else
                    {
                        if (btn.Dock != DockStyle.Top)
                        {
                            btn.Dock = DockStyle.Top;
                            // This is vital to preserve the correct order
                            btn.BringToFront();
                        }
                    }
                }
            }

            // Determine which button was clicked.

            listView1.Items.Clear();
            IteratorIcon(listView1, sitems, clickedButtonTabIndex);
            /*foreach (SidebarItem si in sitems)
            {
                if (si.tabindex == clickedButtonTabIndex && si.subitems!=null) {
                    foreach (SidebarItem_Struct sis in si.subitems)
                    {
                        listView1.Items.Add(sis.title, sis.icontab);
                    }
                    break;
                    
                
                }
            }
            */
            listView1.BringToFront();  // Without this, the buttons will hide the items.
        }
        #endregion Handle events
    }
}
