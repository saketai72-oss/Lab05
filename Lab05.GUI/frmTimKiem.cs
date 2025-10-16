using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Lab05.BUS;
using Lab05.DAL.Entities;
using Lab05.GUI;

namespace Lab05.GUI
{
    public partial class frmTimKiem : Form
    {
        private readonly StudentService studentService = new StudentService();
        public frmTimKiem()
        {
            InitializeComponent();
        }
        private void FillFalcultyCombobox(List<Faculty> listFalcultys)
        {
            // Insert "Tất Cả" at the beginning of the list
            listFalcultys.Insert(0, new Faculty { FacultyID = -1, FacultyName = "Tất Cả" });
            this.cmbFacultyF.DataSource = listFalcultys;
            this.cmbFacultyF.DisplayMember = "FacultyName";
            this.cmbFacultyF.ValueMember = "FacultyID";
        }
        private void BindGrid(List<Student> listStudent)
        {
            dgvStudentF.Rows.Clear();
            foreach (var item in listStudent)
            {
                int index = dgvStudentF.Rows.Add();
                dgvStudentF.Rows[index].Cells[0].Value = item.StudentID;
                dgvStudentF.Rows[index].Cells[1].Value = item.FullName;
                dgvStudentF.Rows[index].Cells[2].Value = item.Faculty.FacultyName;
                dgvStudentF.Rows[index].Cells[3].Value = item.AverageScore;
            }
        }
        private void btnTroVe_Click(object sender, EventArgs e)
        {
            frmStudent form1 = Application.OpenForms["frmStudent"] as frmStudent;
            form1?.Show();
            this.Close();
        }
        public void setGridViewStyle()
        {
            dgvStudentF.BorderStyle = BorderStyle.None;
            dgvStudentF.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            dgvStudentF.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvStudentF.BackgroundColor = Color.White;
            dgvStudentF.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
        private void FindStudent_Load(object sender, EventArgs e)
        {
            dgvStudentF.ColumnCount = 4;
            dgvStudentF.Columns[0].Name = "Mã Số SV";
            dgvStudentF.Columns[1].Name = "Họ Tên";
            dgvStudentF.Columns[2].Name = "Tên Khoa";
            dgvStudentF.Columns[3].Name = "Điểm Trung Bình";

            dgvStudentF.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            try
            {
                setGridViewStyle();
                FillFalcultyCombobox(new StudentModel().Faculties.ToList());
                cmbFacultyF.SelectedIndex = 0; 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnFind_Click(object sender, EventArgs e)
        {
            try
            {
                using (StudentModel context = new StudentModel())
                {
                    var query = context.Students.AsQueryable();
                    if (!string.IsNullOrWhiteSpace(txtStudentIDF.Text))
                    {
                        query = query.Where(s => s.StudentID.Contains(txtStudentIDF.Text));
                    }
                    if (!string.IsNullOrWhiteSpace(txtFullNameF.Text))
                    {
                        query = query.Where(s => s.FullName.Contains(txtFullNameF.Text));
                    }
                    if (cmbFacultyF.SelectedIndex != 0)
                    {
                        int selectedFacultyID = Convert.ToInt32(cmbFacultyF.SelectedValue);
                        query = query.Where(s => s.FacultyID == selectedFacultyID);
                    }
                    txtTong.Text = query.Count().ToString();
                    List<Student> listStudent = query.ToList();
                    BindGrid(listStudent);

                }
            }
            catch
            {

            }
        }
        private void ClearInputFields()
        {
            txtStudentIDF.Clear();
            txtFullNameF.Clear();
            cmbFacultyF.SelectedIndex = 0; // Reset combobox to "Tất Cả"
        }
        private void btnDeleteF_Click(object sender, EventArgs e)
        {
            string studentId = txtStudentIDF.Text.Trim();

            if (string.IsNullOrWhiteSpace(studentId))
            {
                MessageBox.Show("Vui lòng nhập mã sinh viên cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Bạn có chắc muốn xóa sinh viên có mã: {studentId} không?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    StudentService service = new StudentService();
                    service.Delete(studentId);
                    MessageBox.Show("Đã xóa sinh viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Làm mới lại DataGridView
                    ClearInputFields();
                    var list = service.GetAll();
                    BindGrid(list);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa sinh viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void dgvStudentF_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dgvStudentF.Rows[e.RowIndex];
                txtStudentIDF.Text = row.Cells[0].Value.ToString();
                txtFullNameF.Text = row.Cells[1].Value.ToString();
                cmbFacultyF.Text = row.Cells[2].Value.ToString();
            }
        }
    }
}
