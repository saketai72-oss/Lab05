using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lab05.BUS;
using Lab05.DAL.Entities;

namespace Lab05.GUI
{
    public partial class frmRegister : Form
    {
        private readonly StudentService studentService = new StudentService();
        private readonly FacultyService facultyService = new FacultyService();
        private readonly MajorService majorService = new MajorService();
        public frmRegister()
        {
            InitializeComponent();
        }

        private void frmRegister_Load(object sender, EventArgs e)
        {
            dgvMajor.Columns.Clear();

            DataGridViewCheckBoxColumn chkColumn = new DataGridViewCheckBoxColumn();
            chkColumn.Name = "Chọn";
            chkColumn.HeaderText = "Chọn";
            chkColumn.Width = 50;
            dgvMajor.Columns.Add(chkColumn);

            dgvMajor.Columns.Add("MSSV", "MSSV");
            dgvMajor.Columns.Add("HoTen", "Họ Tên");
            dgvMajor.Columns.Add("Khoa", "Khoa");
            dgvMajor.Columns.Add("DTB", "DTB");

            dgvMajor.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            try
            {
                setGridViewStyle();
                var listFacultys = facultyService.GetAll();
                FillFalcultyCombobox(listFacultys);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void setGridViewStyle()
        {
            dgvMajor.BorderStyle = BorderStyle.None;
            dgvMajor.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            dgvMajor.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvMajor.BackgroundColor = Color.White;
            dgvMajor.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
        private void FillFalcultyCombobox(List<Faculty> listFacultys)
        {
            listFacultys.Insert(0, new Faculty { FacultyID = -1, FacultyName = "Tất Cả" });
            this.cmbFaculty.DataSource = listFacultys;
            this.cmbFaculty.DisplayMember = "FacultyName";
            this.cmbFaculty.ValueMember = "FacultyID";
        }
        private void cmbFaculty_SelectedIndexChanged(object sender, EventArgs e)
        {
            Faculty selectedFaculty = cmbFaculty.SelectedItem as Faculty;
            if (selectedFaculty != null)
            {
                if (cmbFaculty.SelectedIndex == 0)
                {
                    cmbMajor.DataSource = null;
                    var listStudents = studentService.GetAllHasNoMajor();
                    BindGrid(listStudents);
                }
                else
                {
                    var listMajor = majorService.GetAllByFaculty(selectedFaculty.FacultyID);
                    FillMajorCombobox(listMajor);
                    var listStudents = studentService.GetAllHasNoMajor(selectedFaculty.FacultyID);
                    BindGrid(listStudents);
                }
            }
        }
        private void FillMajorCombobox(List<Major> listMajors)
        {
            this.cmbMajor.DataSource = listMajors;
            this.cmbMajor.DisplayMember = "Name";
            this.cmbMajor.ValueMember = "MajorID";
        }
        private void BindGrid(List<Student> listStudent)
        {
            dgvMajor.Rows.Clear();
            foreach (var item in listStudent)
            {
                int index = dgvMajor.Rows.Add();
                dgvMajor.Rows[index].Cells[1].Value = item.StudentID;
                dgvMajor.Rows[index].Cells[2].Value = item.FullName;
                if (item.Faculty != null)
                    dgvMajor.Rows[index].Cells[3].Value = item.Faculty.FacultyName;
                dgvMajor.Rows[index].Cells[4].Value = item.AverageScore + "";
                if (item.MajorID != null)
                    dgvMajor.Rows[index].Cells[5].Value = item.Major.Name + "";
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            frmStudent form1 = Application.OpenForms["frmStudent"] as frmStudent;
            form1?.Show();
            this.Close();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (cmbMajor.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn ngành học!");
                return;
            }
            Major selectedMajor = cmbMajor.SelectedItem as Major;
            List<string> selectedStudentIDs = new List<string>();
            foreach (DataGridViewRow row in dgvMajor.Rows)
            {
                DataGridViewCheckBoxCell chk = row.Cells[0] as DataGridViewCheckBoxCell;
                if (chk != null && chk.Value != null && (bool)chk.Value)
                {
                    string studentID = row.Cells[1].Value.ToString();
                    selectedStudentIDs.Add(studentID);
                }
            }
            if (selectedStudentIDs.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn sinh viên để đăng ký ngành học!");
                return;
            }
            try
            {
                studentService.RegisterMajor(selectedStudentIDs, selectedMajor.MajorID);
                MessageBox.Show("Đăng ký ngành học thành công!");
                // Refresh the grid to show updated students
                Faculty selectedFaculty = cmbFaculty.SelectedItem as Faculty;
                if (selectedFaculty != null)
                {
                    var listStudents = studentService.GetAllHasNoMajor(selectedFaculty.FacultyID);
                    BindGrid(listStudents);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đăng ký ngành học: " + ex.Message);
            }
        }
    }
}
