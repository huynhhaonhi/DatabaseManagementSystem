﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLyNhanSu
{
    public partial class FormEmployeeBonus : Form
    {
        private static int? IdEmployeeBonus = null;
        private static int pageIndex;
        private static double Total_Page;
        private static int pageSize;
        public FormEmployeeBonus()
        {
            InitializeComponent();
            pageIndex = 1;
            pageSize = 13;
            Total_Page = 0;
            if (!Connection.IsManager)
            {
                groupBox1.Visible = false;
            }
        }


        private void PaginationEmployeeBonus()
        {
            var request = new PaginationEmployeeBonusRequest();
            request.pageIndex = pageIndex;
            request.pageSize = pageSize;
            if (cbboxDepartmentFilter.SelectedValue == System.DBNull.Value || cbboxDepartmentFilter.SelectedValue == null || cbboxDepartmentFilter.SelectedValue.ToString() == "System.Data.DataRowView")
            {
                request.DepartmentId = null;
            }
            else
            {
                request.DepartmentId = (int)cbboxDepartmentFilter.SelectedValue;
            }
            if (RadioAllDay.Checked == true)
            {
                request.DateBonus = null;
            }
            else
            {
                request.DateBonus = DayFilter.Value;
            }
            request.NameSearch = txtSearch.Text.Length == 0 ? null : txtSearch.Text.Trim();



            var result = Utilities.EmployeeBonusAll(request);
            //Fill Data Return to DataGrid , Remove Column Total
            GridEmployeeBonus.DataSource = result.Select(c => new {
                Id = c.Id,
                EmployeeId = c.EmployeeId,
                FullName = c.FullName,
                DepartmentName = c.DepartmentName,
                DateBonus = c.DateBonus,
                BonusName = c.BonusName,
                ValueBonus = c.ValueBonus,
            }).ToList();

            //Get pagination result (Current page & Total page)
            var pagination = Utilities.Pagination<EmployeeBonusViewModel>(result, request);

            //Set globals Total page
            Total_Page = pagination.TotalPage;

            //Set Display pagination UI
            LblPageIndex.Text = pageIndex + " / " + Total_Page;

            //if else ..
            if (pageIndex == 1) // you are stay top page
            {
                btnBackpage.Enabled = false;
                btnBeginPage.Enabled = false;
            }
            else
            {
                btnBackpage.Enabled = true;
                btnBeginPage.Enabled = true;
            }

            if (pageIndex == Total_Page) // you are stay last page
            {
                BtnNextPage.Enabled = false;
                BtnEndPage.Enabled = false;
            }
            else
            {
                BtnNextPage.Enabled = true;
                BtnEndPage.Enabled = true;
            }

            if (Total_Page == 0 && pageIndex > 0)
            {
                if (pageIndex == 1)
                {
                    return;
                }
                else
                {
                    pageIndex--;
                    PaginationEmployeeBonus();
                }
            }
        }

        private bool ValidateEmployeeBonus()
        {
            if (txtIdEmployee.Text.Length == 0 || cbboxFullName.DataSource == null)
            {
                labelRequiredName.Visible = true;
                return false;
            }
            else
            {
                labelRequiredName.Visible = false;
                return true;
            }
        }

        private void FrmEmployeeBonus_Load(object sender, EventArgs e)
        {
            RadioAllDay.Checked = true;
            DayFilter.Enabled = false;

            var departments = Utilities.Departments();

            var dropdown = Utilities.BuildDropDownlist(departments, 0, 0);


            var departmentsForm = dropdown;

            cbboxDepartmentForm.DataSource = departmentsForm;
            cbboxDepartmentForm.DisplayMember = "Name";
            cbboxDepartmentForm.ValueMember = "Id";

            var departmentsFilter = Utilities.CollectionToDataTableDefaultSelect<NodeViewModel>(dropdown);
            DataRow DefaultSelectFilter = departmentsFilter.NewRow();
            DefaultSelectFilter[0] = DBNull.Value;
            DefaultSelectFilter[1] = "== Tất cả đơn vị,phòng ban ==";

            departmentsFilter.Rows.InsertAt(DefaultSelectFilter, 0);


            cbboxDepartmentFilter.DataSource = departmentsFilter;
            cbboxDepartmentFilter.DisplayMember = "Name";
            cbboxDepartmentFilter.ValueMember = "Id";

            var Bonus = Utilities.AllBonus();
            cbboxBonus.DataSource = Bonus;
            cbboxBonus.DisplayMember = "Name";
            cbboxBonus.ValueMember = "Id";
            PaginationEmployeeBonus();
            CreateModel();
        }

        private void cbboxDepartmentForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            var t = cbboxDepartmentForm.SelectedValue.ToString();
            if (t == "System.Data.DataRowView" || t == "QuanLyNhanSu.NodeViewModel")
            {
                return;
            }
            var request = new PaginationEmployeeRequest()
            {
                pageIndex = 1,
                pageSize = 0,
                DepartmentId = int.Parse(t),
                NameSearch = null,
                IsWorking = true
            };
            var EmployeeByDepartment = Utilities.Employees(request);
            if (EmployeeByDepartment.Count == 0)
            {
                cbboxFullName.DataSource = null;
            }
            else
            {
                cbboxFullName.DataSource = EmployeeByDepartment;
                cbboxFullName.DisplayMember = "FullName";
                cbboxFullName.ValueMember = "Id";
            }
        }

        private void cbboxFullName_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cbboxFullName_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbboxFullName.DataSource != null && cbboxFullName.SelectedValue != null)
            {
                var t = cbboxFullName.SelectedValue.ToString();
                if (t == "System.Data.DataRowView" || t == "QuanLyNhanSu.EmployeeViewModel")
                {
                    return;
                }
                txtIdEmployee.Text = t;
                ValidateEmployeeBonus();
            }
        }

        private void CreateModel()
        {
            IdEmployeeBonus = null;
            txtIdEmployee.Text = null;
            cbboxFullName.DataSource = null;
            BtnCreate.Visible = false;
            btnDelete.Visible = false;
            cbboxFullName.Enabled = true;
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            CreateModel();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {

            if (IdEmployeeBonus == null)
            {
                MessageBox.Show("Không có đối tượng cần xóa");
            }
            else
            {
                DialogResult Notification;
                Notification = MessageBox.Show("Delete", "Bạn thực sự muốn xóa?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (Notification == DialogResult.OK)
                {
                    using (SqlConnection con = new SqlConnection(Connection.GetString(Connection.IsManager)))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "EmployeeBonusDelete";
                        cmd.Parameters.Add(new SqlParameter("@Id", IdEmployeeBonus));
                        var x = cmd.ExecuteNonQuery();
                        if (x == 1)
                        {
                            MessageBox.Show("Delete Success");
                        }
                        else
                        {
                            MessageBox.Show("Delete Failed");
                        }
                        con.Close();
                    }
                    CreateModel();
                    PaginationEmployeeBonus();
                }

            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (ValidateEmployeeBonus() == false)
            {
                return;
            }
            else
            {
                using (SqlConnection con = new SqlConnection(Connection.GetString(Connection.IsManager)))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "EmployeeBonusSave";
                    if (IdEmployeeBonus == null)
                    {
                        cmd.Parameters.Add(new SqlParameter("@Id", DBNull.Value));
                    }
                    else
                    {
                        cmd.Parameters.Add(new SqlParameter("@Id", IdEmployeeBonus));
                    }
                    cmd.Parameters.Add(new SqlParameter("@IdEmployee", int.Parse(txtIdEmployee.Text)));
                    cmd.Parameters.Add(new SqlParameter("@IdBonus", (int)cbboxBonus.SelectedValue));
                    cmd.Parameters.Add(new SqlParameter("@DateBonus", DateBonus.Value));
                    var x = cmd.ExecuteNonQuery();
                    if (x == 1)
                    {
                        MessageBox.Show("Saved Success");
                    }
                    else
                    {
                        MessageBox.Show("Saved Failed");
                    }
                    con.Close();
                }
                PaginationEmployeeBonus();
                CreateModel();
                BtnCreate.Visible = false;
                btnDelete.Visible = false;
            }
        }

        private void RadioAllDay_CheckedChanged(object sender, EventArgs e)
        {
            DayFilter.Enabled = false;
            PaginationEmployeeBonus();
        }

        private void radioDay_CheckedChanged(object sender, EventArgs e)
        {
            DayFilter.Enabled = true;
            PaginationEmployeeBonus();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            PaginationEmployeeBonus();
        }

        private void GridEmployeeBonus_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var Entity = GridEmployeeBonus.CurrentRow.Cells["IdCL"].Value.ToString();

            var result = Utilities.GetEmployeeBonusById(int.Parse(Entity));

            if (result.Rows.Count != 0)
            {
                cbboxDepartmentForm_SelectedIndexChanged(sender, e);
                foreach (DataRow data in result.Rows)
                {
                    IdEmployeeBonus = (int)data["Id"];
                    txtIdEmployee.Text = data["EmployeeId"].ToString();
                    cbboxDepartmentForm.SelectedValue = (int)data["DepartmentId"];
                    cbboxBonus.SelectedValue = (int)data["BonusId"];
                    cbboxFullName.SelectedValue = (int)data["EmployeeId"];
                    DateBonus.Value = (DateTime)data["DateBonus"];
                }
                cbboxFullName.Enabled = false;
                BtnCreate.Visible = true;
                btnDelete.Visible = true;
            }
            else
            {
                MessageBox.Show("Get Employee Failed");
            }
        }

        private void cbboxDepartmentForm_SelectedValueChanged(object sender, EventArgs e)
        {

        }

        private void btnBeginPage_Click(object sender, EventArgs e)
        {
            if (Total_Page == 0)
            {
                return;
            };
            if (pageIndex == 1)
            {
                MessageBox.Show("Bạn đang ở trang đầu tiên");
            }
            else
            {
                pageIndex = 1;
                PaginationEmployeeBonus();
            }
        }

        private void btnBackpage_Click(object sender, EventArgs e)
        {
            if (Total_Page == 0)
            {
                return;
            };
            if (pageIndex == 1)
            {
                MessageBox.Show("Bạn đang ở trang đầu tiên");
            }
            else
            {
                pageIndex--;
                PaginationEmployeeBonus();
            }
        }

        private void BtnNextPage_Click(object sender, EventArgs e)
        {
            if (Total_Page == 0)
            {
                return;
            };
            if (pageIndex == Total_Page)
            {
                MessageBox.Show("Bạn đang ở trang cuối cùng");
            }
            else
            {
                pageIndex++;
                PaginationEmployeeBonus();
            }
        }

        private void BtnEndPage_Click(object sender, EventArgs e)
        {
            if (Total_Page == 0)
            {
                return;
            };
            if (pageIndex == Total_Page)
            {
                MessageBox.Show("Bạn đang ở trang cuối cùng");
            }
            else
            {
                pageIndex = (int)Total_Page;
                PaginationEmployeeBonus();
            }
        }

        private void cbboxDepartmentFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            PaginationEmployeeBonus();
        }

        private void DayFilter_ValueChanged(object sender, EventArgs e)
        {
            PaginationEmployeeBonus();
        }
    }
}
