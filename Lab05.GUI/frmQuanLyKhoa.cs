using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Lab05.BUS;
using Lab05.DAL.Entities;
using Lab05.GUI;

namespace Lab05.GUI
{
    public partial class frmQuanLyKhoa : Form
    {
        private readonly FacultyService facultyService = new FacultyService();
        public frmQuanLyKhoa()
        {
            InitializeComponent();
        }

        private void btnAdd_Update_Click(object sender, EventArgs e)
        {
            try
            {
                using (StudentModel db = new StudentModel())
                {
                    string facultyIDText = txtFacultyID.Text.Trim();
                    string facultyName = txtFacultyName.Text.Trim();

                    if (string.IsNullOrWhiteSpace(facultyIDText) ||
                        string.IsNullOrWhiteSpace(facultyName))
                    {
                        throw new Exception("Vui lòng nhập đầy đủ thông tin: Mã Khoa và Tên Khoa!");
                    }
                    if (!Regex.IsMatch(facultyIDText, @"^\d+$"))
                        throw new Exception("Mã Khoa phải là số nguyên dương!");

                    int facultyID = int.Parse(facultyIDText);

                    if (!Regex.IsMatch(facultyName, @"^[a-zA-ZÀ-ỹ\s]{3,100}$"))
                        throw new Exception("Tên Khoa chỉ được chứa chữ cái và khoảng trắng, không chứa số hoặc ký tự đặc biệt, từ 3–100 ký tự.");

                    var existingFaculty = db.Faculties.FirstOrDefault(f => f.FacultyID == facultyID);

                    if (existingFaculty != null)
                    {
                        existingFaculty.FacultyName = facultyName;

                        db.SaveChanges();
                        MessageBox.Show("Cập nhật thông tin khoa thành công!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        Faculty newFaculty = new Faculty()
                        {
                            FacultyID = facultyID,
                            FacultyName = facultyName,
                        };

                        db.Faculties.Add(newFaculty);
                        db.SaveChanges();
                        MessageBox.Show("Thêm khoa mới thành công!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    BindGrid(db.Faculties.ToList());
                    txtTongKhoa.Text = db.Faculties.Count().ToString();

                    ClearInput();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BindGrid(List<Faculty> listFaculty)
        {
            dgvFaculty.Rows.Clear();
            foreach (var item in listFaculty)
            {
                int index = dgvFaculty.Rows.Add();
                dgvFaculty.Rows[index].Cells[0].Value = item.FacultyID;
                dgvFaculty.Rows[index].Cells[1].Value = item.FacultyName;
            }
        }
        public void setGridViewStyle()
        {
            dgvFaculty.BorderStyle = BorderStyle.None;
            dgvFaculty.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            dgvFaculty.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvFaculty.BackgroundColor = Color.White;
            dgvFaculty.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
        private void QuanLyKhoa_Load(object sender, EventArgs e)
        {
            dgvFaculty.ColumnCount = 2;
            dgvFaculty.Columns[0].Name = "Mã Khoa";
            dgvFaculty.Columns[1].Name = "Tên Khoa";

            dgvFaculty.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            try
            {
                setGridViewStyle();
                using (StudentModel context = new StudentModel())
                {
                    List<Faculty> listFaculties = context.Faculties.ToList();
                    BindGrid(listFaculties);
                    txtTongKhoa.Text = context.Faculties.Count().ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi khi tải dữ liệu");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                using (StudentModel db = new StudentModel())
                {
                    if (dgvFaculty.SelectedRows.Count == 0)
                    {
                        MessageBox.Show("Vui lòng chọn một khoa để xóa!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int facultyId = Convert.ToInt32(dgvFaculty.SelectedRows[0].Cells[0].Value);
                    var facultyToDelete = db.Faculties.FirstOrDefault(f => f.FacultyID == facultyId);

                    if (facultyToDelete == null)
                    {
                        MessageBox.Show("Không tìm thấy khoa cần xóa!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    DialogResult result = MessageBox.Show("Bạn có chắc muốn xóa khoa này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No)
                        return;

                    db.Faculties.Remove(facultyToDelete);
                    db.SaveChanges();

                    BindGrid(db.Faculties.ToList());
                    txtTongKhoa.Text = db.Faculties.Count().ToString();
                    ClearInput();

                    MessageBox.Show("Xóa khoa thành công!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa khoa: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvFaculty_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvFaculty.Rows[e.RowIndex];
                txtFacultyID.Text = row.Cells[0].Value.ToString();
                txtFacultyName.Text = row.Cells[1].Value.ToString();
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            frmStudent form1 = Application.OpenForms["frmStudent"] as frmStudent;
            form1?.Show();
            this.Close();
        }

        private void ClearInput()
        {
            txtFacultyID.Text = "";
            txtFacultyName.Text = "";
        }
    }
}
