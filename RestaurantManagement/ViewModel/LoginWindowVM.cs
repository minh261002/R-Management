﻿using Project;
using QuanLyNhaHang.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using QuanLyNhaHang;
using System.Data.SqlTypes;

namespace RestaurantManagement.ViewModel
{
    public class LoginWindowVM : BaseViewModel
    {
        private string strCon = ConfigurationManager.ConnectionStrings["QuanLyNhaHang"].ConnectionString;
        private SqlConnection sqlCon = null;
        public bool IsLoggedIn { get; set; }
        private string _UserName;
        private static string _MaNV;
        public string UserName { get => _UserName; set { _UserName = value; OnPropertyChanged(); } }
        private string _Password;
        public string Password { get => _Password; set { _Password = value; OnPropertyChanged(); } }
        public static string MaNV { get => _MaNV; set { _MaNV = value; } }
        public string Role { get; set; }
        public ICommand CloseLoginCM { get; set; }
        public ICommand LoginCM { get; set; }
        public ICommand PasswordChangedCommand { get; set; }
        public LoginWindowVM()
        {
            IsLoggedIn = false;
            CloseLoginCM = new RelayCommand<Window>((p) => { return true; }, (p) =>
            {
                if (p == null) return;
                p.Close();
            });
            PasswordChangedCommand = new RelayCommand<PasswordBox>((p) => { return true; }, (p) => { Password = p.Password; });
            LoginCM = new RelayCommand<Window>((p) => { return true; }, (p) =>
            {
                Login(p);
                if (IsLoggedIn)
                {
                    p.Close();
                    return;
                }
                else
                {
                    MyMessageBox msb = new MyMessageBox("Sai tên đăng nhập hoặc mật khẩu!");
                    msb.ShowDialog();
                }
            });
            void Login(Window p)
            {
                OpenConnect();

                if (p == null) return;

                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM TAIKHOAN WHERE ID = '" + UserName + "' AND MatKhau = '" + Password + "'";
                cmd.Connection = sqlCon;
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IsLoggedIn = true;
                    Role = reader.GetString(2);
                    MaNV = reader.GetString(3);
                }
                reader.Close();

                CloseConnect();
            }
            void OpenConnect()
            {
                sqlCon = new SqlConnection(strCon);
                if (sqlCon.State == ConnectionState.Closed)
                {
                    sqlCon.Open();
                }
            }

            void CloseConnect()
            {
                if (sqlCon.State == ConnectionState.Open)
                {
                    sqlCon.Close();
                }
            }
        }
    }
}
