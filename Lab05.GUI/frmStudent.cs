using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Lab05.BUS;
using Lab05.DAL.Entities;

namespace Lab05.GUI
{
    public partial class frmStudent : Form
    {
        private readonly StudentService studentService = new StudentService();
        private readonly FacultyService facultyService = new FacultyService();
        public frmStudent()
        {
            InitializeComponent();
        }

        private void frmStudent_Load(object sender, EventArgs e)
        {
            dgvStudent.ColumnCount = 5;

            dgvStudent.Columns[0].Name = "MSSV";
            dgvStudent.Columns[1].Name = "Họ Tên";
            dgvStudent.Columns[2].Name = "Khoa";
            dgvStudent.Columns[3].Name = "DTB";
            dgvStudent.Columns[4].Name = "Chuyên Ngành";

            dgvStudent.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            txtID.MaxLength = 10;
            try
            {
                setGridViewStyle();
                var listFacultys = facultyService.GetAll();
                var listStudents = studentService.GetAll();
                FillFalcultyCombobox(listFacultys);
                cmbFaculty.SelectedIndex = 1;
                BindGrid(listStudents);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void FillFalcultyCombobox(List<Faculty> listFacultys)
        {
            listFacultys.Insert(0, new Faculty());
            this.cmbFaculty.DataSource = listFacultys;
            this.cmbFaculty.DisplayMember = "FacultyName";
            this.cmbFaculty.ValueMember = "FacultyID";
        }
        private void BindGrid(List<Student> listStudent)
        {
            dgvStudent.Rows.Clear();
            foreach (var item in listStudent)
            {
                int index = dgvStudent.Rows.Add();
                dgvStudent.Rows[index].Cells[0].Value = item.StudentID;
                dgvStudent.Rows[index].Cells[1].Value = item.FullName;
                if (item.Faculty != null)
                    dgvStudent.Rows[index].Cells[2].Value = item.Faculty.FacultyName;
                dgvStudent.Rows[index].Cells[3].Value = item.AverageScore + "";
                if (item.MajorID != null)
                    dgvStudent.Rows[index].Cells[4].Value = item.Major.Name + "";
                LoadAvatar(item.StudentID);
            }
        }
        public void setGridViewStyle()
        {
            dgvStudent.BorderStyle = BorderStyle.None;
            dgvStudent.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            dgvStudent.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvStudent.BackgroundColor = Color.White;
            dgvStudent.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
        private string avatarFilePath = string.Empty;
        private void btnUpload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files (*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    avatarFilePath = openFileDialog.FileName;
                    picAvatar.Image = Image.FromFile(avatarFilePath);
                }
            }
        }
        private void LoadAvatar(string studentID)
        {
            const string ImagesFolderName = "Images";
            string folderPath = Path.Combine(Application.StartupPath, ImagesFolderName);

            var student = studentService.FindById(studentID);
            if (student == null || string.IsNullOrEmpty(student.Avatar))
            {
                picAvatar.Image = null;
                return;
            }

            string avatarFilePath = Path.Combine(folderPath, student.Avatar);
            if (File.Exists(avatarFilePath))
            {
                try
                {
                    if (picAvatar.Image != null)
                    {
                        picAvatar.Image.Dispose();
                        picAvatar.Image = null;
                    }
                    using (var avatarImage = Image.FromFile(avatarFilePath))
                    {
                        picAvatar.Image = new Bitmap(avatarImage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading avatar: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    picAvatar.Image = null;
                }
            }
            else
                picAvatar.Image = null;
        }
        private string SaveAvatar(string sourceFilePath, string studentID)
        {
            const string ImagesFolderName = "Images";
            try
            {
                if (string.IsNullOrWhiteSpace(sourceFilePath) || string.IsNullOrWhiteSpace(studentID))
                {
                    throw new ArgumentException("Source file path and student ID must not be null or empty.");
                }

                if (!File.Exists(sourceFilePath))
                {
                    throw new FileNotFoundException($"Source file not found: {sourceFilePath}");
                }

                string folderPath = Path.Combine(Application.StartupPath, ImagesFolderName);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileExtension = Path.GetExtension(sourceFilePath);
                string targetFileName = $"{studentID}{fileExtension}";
                string targetFilePath = Path.Combine(folderPath, targetFileName);

                File.Copy(sourceFilePath, targetFilePath, overwrite: true);

                return targetFileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving avatar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }
        private void btnAdd_Update_Click(object sender, EventArgs e)
        {
            try
            {
                var student = studentService.FindById(txtID.Text) ?? new Student();
                student.StudentID = txtID.Text.Trim();
                student.FullName = txtName.Text.Trim();
                student.AverageScore = double.Parse(txtAverageScore.Text);
                student.FacultyID = Convert.ToInt32(cmbFaculty.SelectedValue.ToString());

                if (!string.IsNullOrEmpty(avatarFilePath))
                {
                    string avatarFileName = SaveAvatar(avatarFilePath, txtID.Text);
                    if (!string.IsNullOrEmpty(avatarFileName))
                    {
                        student.Avatar = avatarFileName;
                    }
                }

                studentService.InsertUpdate(student);
                MessageBox.Show("Thêm / Cập nhật sinh viên mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                BindGrid(studentService.GetAll());

                avatarFilePath = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding data: {ex.Message}", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void chkUnregisterMajor_CheckedChanged(object sender, EventArgs e)
        {
            var listStudents = new List<Student>();
            if (this.chkUnregisterMajor.Checked)
                listStudents = studentService.GetAllHasNoMajor();
            else
                listStudents = studentService.GetAll();
            BindGrid(listStudents);
        }
        private void tsQuanLyKhoa_Click(object sender, EventArgs e)
        {
            frmQuanLyKhoa frm = new frmQuanLyKhoa();
            frm.Show();
            Hide();
        }
        private void tsFind_Click(object sender, EventArgs e)
        {
            frmTimKiem frm = new frmTimKiem();
            frm.Show();
            Hide();
        }

        private void tsRegister_Click(object sender, EventArgs e)
        {
            frmRegister frm = new frmRegister();
            frm.Show();
            Hide();
        }
        private void dgvStudent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvStudent.Rows[e.RowIndex];
                txtID.Text = row.Cells[0].Value?.ToString();
                txtName.Text = row.Cells[1].Value?.ToString();
                if (row.Cells[2].Value != null)
                    cmbFaculty.Text = row.Cells[2].Value.ToString();
                txtAverageScore.Text = row.Cells[3].Value?.ToString();
                LoadAvatar(row.Cells[0].Value.ToString());
            }
        }
        private void btnOut_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void ClearInputFields()
        {
            txtID.Clear();
            txtName.Clear();
            txtAverageScore.Clear();
            cmbFaculty.SelectedIndex = 0;
            picAvatar.Image = null;
            avatarFilePath = string.Empty; 
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            string studentId = txtID.Text.Trim();

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

        
    }
}

