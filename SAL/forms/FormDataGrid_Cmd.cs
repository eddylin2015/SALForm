﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAL.forms
{
    public partial class FormDataGrid_Cmd : Form
    {

        private bool InitializeFlag = false;
        private string DefaultFileName = null;
        private idgvEditCP edite_func = dgvEditCP.GetInst;
        protected iFormGrid idg;
        protected Hashtable dict = null;
        public BindingSource customersBindingSource = new BindingSource();
        public FormDataGrid_Cmd(iFormGrid idg, Hashtable adict, BindingListOptions bloption):base()
        {
            InitializeComponent();
            this.idg = idg;
            this.dict = adict;
            switch (bloption)
            {
                case BindingListOptions.AllowNewNo:
                    this.customersBindingSource.AllowNew = false;
                    break;
                case BindingListOptions.AllowModifyNo:
                    this.customersBindingSource.AllowNew = false;
                    this.dataGridView1.ReadOnly = true;
                    break;
                case BindingListOptions.AllPri:
                    break;
            }
            Initialize();
            pasteclick.Click += new EventHandler(gridpaste_Click);
            copyclick.Click += new EventHandler(copyGrid_Click);
            dataGridView1.CellMouseClick += new DataGridViewCellMouseEventHandler(m_Grid_CellMouseClick);
            this.dataGridView1.ContextMenuStrip = this.contextMenuStrip1;
            this.tls_save.Click += Save_Click;
            this.tlSLoadNext.Click+= LoadNext_Click;
            this.tslReadOnly.Click += ReadOnly_Click;
            this.tlsXls.Click += ExpXls_Click;
            
	    this.tlsAddRow.Click += tlsAddRow_Click;
            this.tlsCalc.Click += tlsCalc_Click;
            this.tlsTotal.Click += tlsTotal_Click;
            this.tlsPrtSal.Click += tlsPrt_Click;
        }
        public void tlsPrt_Click(Object sender, EventArgs e) {
            idg.PritnSALForm();
        }
        public void tlsCalc_Click(Object sender,EventArgs e)
        {
            bool flag = this.dataGridView1.ReadOnly;
            this.dataGridView1.ReadOnly = true;
            idg.Calc();
            this.dataGridView1.ReadOnly =flag;
        }
        public void FrozenLeftColumns(int cnt, bool ReadOnly)
        {
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                if (i < cnt)
                {
                    dataGridView1.Columns[i].Frozen = true;
                    dataGridView1.Columns[i].ReadOnly = ReadOnly;
                }
                else
                {
                    dataGridView1.Columns[i].Frozen = false;
                    dataGridView1.Columns[i].ReadOnly = false;
                }
            }
        }
        public void InitializeAfterConstructor()
        {
            if (!InitializeFlag)
                Initialize();
        }
        private void Initialize()
        {
            InitializeFlag = true;
            this.bindingNavigator1.BindingSource = this.customersBindingSource;
            this.customersBindingSource.DataSource = idg.GetDT();
            this.dataGridView1.DataSource = this.customersBindingSource;
            if (dict == null) return;
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                if (dict.Contains(dataGridView1.Columns[i].Name)){
                    dataGridView1.Columns[i].HeaderCell.Value = dict[dataGridView1.Columns[i].Name];
                }
                else if (dict.Contains(dataGridView1.Columns[i].Name.ToLower()))
                    dataGridView1.Columns[i].HeaderCell.Value = dict[dataGridView1.Columns[i].Name.ToLower()];
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                if (dataGridView1.Columns[i].ValueType.ToString() == "System.Int32"
                    || dataGridView1.Columns[i].ValueType.ToString() == "System.Int16"
                    || dataGridView1.Columns[i].ValueType.ToString() == "System.Decimal"
                    || dataGridView1.Columns[i].ValueType.ToString() == "System.Single"
                    || dataGridView1.Columns[i].ValueType.ToString() == "System.Double")
                {
                    dataGridView1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }
        private void ReadOnly_Click(object sender, EventArgs e)
        {
            this.dataGridView1.ReadOnly = !this.dataGridView1.ReadOnly;
        }
        private void LoadNext_Click(object sender, EventArgs e)
        {
            this.idg.FillDT_Next_int();
        }
        
            
        private void Save_Click(object sender, EventArgs e)
        {
            CurrencyManager fcm;
            fcm = (CurrencyManager)this.BindingContext[this.dataGridView1.DataSource];
            fcm.EndCurrentEdit();
            // int i=adapter.Update(ds);
            this.dataGridView1.ReadOnly = true;
            int i = idg.submitUpdate();
            this.dataGridView1.ReadOnly = false;
            this.dataGridView1.Focus();
            // PubDialog.ShowUpdateCnt(i);
        }
        private void ExpXls_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.FileName = "*.xls";
            if (DefaultFileName != null) sf.FileName = DefaultFileName;
            if (sf.ShowDialog() == DialogResult.OK)
            {
                idg.Exportxls(sf.FileName, dict);
                if (MessageBox.Show("是否打開文件" + sf.FileName, "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    ShellExec.ShellExecute(IntPtr.Zero, new StringBuilder("Open"), new StringBuilder(sf.FileName), new StringBuilder(""), new StringBuilder(""), 1);
            }
        }
        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            //
        }
        void m_Grid_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                this.contextMenuStrip1.Visible = true;
        }
        void copyGrid_Click(object sender, EventArgs e)
        {
            edite_func.copyGrid_Click(this.dataGridView1);
        }
        void gridpaste_Click(object sender, EventArgs e)
        {
            edite_func.PasteClipboard(this.dataGridView1);
        }

        private void tlsAddRow_Click(object sender, EventArgs e)
        {
            idg.AddNumOfRows();
        }
        private void tlsTotal_Click(object sender, EventArgs e)
        {
            idg.CalcTotal();
        }
        private void tlsFZ_Click(object sender, EventArgs e)
        {
            string str = forms.utls.prompt("涷結左邊欄位", "Add Num Of column:", "1");
            string pat = @"^\d+$";
            if (str != null && Regex.IsMatch(str, pat))
            {
                FrozenLeftColumns( int.Parse(str), true);
            }
        }
       
    }
}
