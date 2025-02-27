﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using StudentAttendanceMgr.CommonClass;

namespace StudentAttendanceMgr.StudentService
{
    public partial class frmSetStudentInfo : Form
    {
        // 照片文件的路径
        private string photoFileName = "";
        
        public frmSetStudentInfo()
        {
            InitializeComponent();
        }

        // 验证用户输入
        private bool ValidateInput()
        {
            if (this.txtStuName.Text.Trim() == "")
            {
                MessageBox.Show("请输入姓名", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.txtStuName.Focus();
                return false;
            }
            else if (this.cboSex.Text.Trim() == "")
            {
                MessageBox.Show("请输入性别", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.cboSex.Focus();
                return false;
            }
            else if (this.txtTelephone.Text.Trim() == "")
            {
                MessageBox.Show("请输入联系电话", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.txtTelephone.Focus();
                return false;
            }
            else if (this.txtHomeAddress.Text.Trim() == "")
            {
                MessageBox.Show("请输入家庭住址", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.txtHomeAddress.Focus();
                return false;
            }
            else if (this.txtHomePhone.Text.Trim() == "")
            {
                MessageBox.Show("请输入家庭电话", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.txtHomePhone.Focus();
                return false;
            }

            return true;
        }

        // 窗体加载
        private void frmSetStudentInfo_Load(object sender, EventArgs e)
        {
            // 获取当前登录学生的个人资料
            try
            {
                // 查询记录用的SQL语句
                string selectSql = String.Format("select StuId, StuName, Sex, Photo, ClassName, DormName, Telephone, HomeAddress, HomePhone " +
                    "from Students s, Classes c, Dorms d " +
                    "where s.ClassId = c.ClassId and s.DormId = d.DormId and StuId = '{0}'", CommonInfo.userId);

                SqlCommand command = new SqlCommand(selectSql, DBHelper.connection);

                if (DBHelper.connection.State == ConnectionState.Closed)
                {
                    DBHelper.connection.Open();
                }

                SqlDataReader sdr = command.ExecuteReader();

                if (sdr.HasRows)
                {
                    sdr.Read();
                    this.txtStuId.Text = sdr["StuId"].ToString();
                    this.txtStuName.Text = sdr["StuName"].ToString();
                    this.cboSex.Text = sdr["Sex"].ToString();
                    this.txtClassName.Text = sdr["ClassName"].ToString();
                    this.txtDormName.Text = sdr["DormName"].ToString();
                    this.txtTelephone.Text = sdr["Telephone"].ToString();
                    this.txtHomeAddress.Text = sdr["HomeAddress"].ToString();
                    this.txtHomePhone.Text = sdr["HomePhone"].ToString();

                    //从Students表中取出学生的个人照片，判断是否为NULL值，如果不为NULL值，则显示在图片框控件中。

                    if (sdr.IsDBNull(3) == false)  // Photo是第四列，索引为3。
                    {
                        //创建一个字节数组
                        Byte[] photo = new Byte[0];

                        //将取出来的照片存储在字节数组中
                        photo = (Byte[])sdr["Photo"];

                        //根据字节数组，创建一个内存流对象。
                        MemoryStream ms = new MemoryStream(photo);

                        //将照片显示在图片框控件中
                        this.picPhoto.Image = Image.FromStream(ms);

                        ms.Close();
                    } 
                }

                if (sdr.IsClosed == false)
                {
                    sdr.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (DBHelper.connection.State != ConnectionState.Closed)
                {
                    DBHelper.connection.Close();
                }
            }
        }

        // 更改照片
        private void btnBrowser_Click(object sender, EventArgs e)
        {
            //弹出打开文件对话框，选择一幅照片
            OpenFileDialog openDlg = new OpenFileDialog();

            DialogResult dr = openDlg.ShowDialog();

            if (dr == DialogResult.OK)
            {
                //获取文件路径
                photoFileName = openDlg.FileName;

                //将照片显示在图片框控件中
                this.picPhoto.Image = Image.FromFile(photoFileName);
            }
        }

        // 确定
        private void btnOK_Click(object sender, EventArgs e)
        {
            //验证用户输入，如果输入有误，则终止操作。
            if (this.ValidateInput() == false)
            {
                return;
            }

            int updateResult = 0;  //操作结果

            try
            {
                //判断是否更改了照片，如果没有更改照片，则不用修改 Photo 列的值。
                if (photoFileName == "")
                {
                    //修改记录用的SQL语句
                    string updateSql = "update Students set StuName = @StuName, Sex = @Sex, Telephone = @Telephone, HomeAddress = @HomeAddress, HomePhone = @HomePhone where StuId = @StuId";

                    //创建 Command 对象
                    SqlCommand command = new SqlCommand(updateSql, DBHelper.connection);

                    //指定SQL语句中各参数的值
                    command.Parameters.Add(new SqlParameter("@StuName", this.txtStuName.Text));
                    command.Parameters.Add(new SqlParameter("@Sex", this.cboSex.Text));
                    command.Parameters.Add(new SqlParameter("@Telephone", this.txtTelephone.Text));
                    command.Parameters.Add(new SqlParameter("@HomeAddress", this.txtHomeAddress.Text));
                    command.Parameters.Add(new SqlParameter("@HomePhone", this.txtHomePhone.Text));
                    command.Parameters.Add(new SqlParameter("@StuId", this.txtStuId.Text));

                    //打开数据库连接
                    if (DBHelper.connection.State == ConnectionState.Closed)
                    {
                        DBHelper.connection.Open();
                    }

                    //执行修改操作，返回本次操作所影响的记录行数。
                    updateResult = command.ExecuteNonQuery();
                }
                else
                {
                    //读取照片文件到文件流
                    FileStream fs = new FileStream(photoFileName, FileMode.Open, FileAccess.Read);

                    //通过文件流创建一个字节数组
                    Byte[] photo = new Byte[fs.Length];

                    //从流中读取照片文件，并保存到字节数组中。
                    fs.Read(photo, 0, Convert.ToInt32(fs.Length));

                    //关闭文件流
                    fs.Close();


                    //修改记录用的SQL语句
                    string updateSql = "update Students set StuName = @StuName, Sex = @Sex, Photo = @Photo, Telephone = @Telephone, HomeAddress = @HomeAddress, HomePhone = @HomePhone where StuId = @StuId";

                    //创建 Command 对象
                    SqlCommand command = new SqlCommand(updateSql, DBHelper.connection);

                    //指定SQL语句中各参数的值
                    command.Parameters.Add(new SqlParameter("@StuName", this.txtStuName.Text));
                    command.Parameters.Add(new SqlParameter("@Sex", this.cboSex.Text));
                    command.Parameters.Add(new SqlParameter("@Photo", photo));
                    command.Parameters.Add(new SqlParameter("@Telephone", this.txtTelephone.Text));
                    command.Parameters.Add(new SqlParameter("@HomeAddress", this.txtHomeAddress.Text));
                    command.Parameters.Add(new SqlParameter("@HomePhone", this.txtHomePhone.Text));
                    command.Parameters.Add(new SqlParameter("@StuId", this.txtStuId.Text));

                    //打开数据库连接
                    if (DBHelper.connection.State == ConnectionState.Closed)
                    {
                        DBHelper.connection.Open();
                    }

                    //执行修改操作，返回本次操作所影响的记录行数。
                    updateResult = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //关闭数据库连接
                if (DBHelper.connection.State != ConnectionState.Closed)
                {
                    DBHelper.connection.Close();
                }
            }

            if (updateResult >= 1)
            {
                MessageBox.Show("更新个人资料成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // 取消
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();  //关闭窗体
        }
    }
}