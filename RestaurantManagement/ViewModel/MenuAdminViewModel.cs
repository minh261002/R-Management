﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using QuanLyNhaHang.DataProvider;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.View;
using System.Windows.Data;
using System.ComponentModel;
using Diacritics.Extensions;
using System.Data.SqlClient;
using System.Diagnostics;
using Button = System.Windows.Controls.Button;

namespace QuanLyNhaHang.ViewModel
{
    public class MenuAdminViewModel : BaseViewModel
    {
        public MenuAdminViewModel()
        {
                LoadMenu();
                Ingredients = MenuDP.Flag.GetIngredients();
                Ingredients_ForDishes = new ObservableCollection<ChiTietMon>();
                Deleted_Ingredients = new ObservableCollection<ChiTietMon>();
                _menuItemsView = new CollectionViewSource();
                _menuItemsView.Source = MenuItems;
                _menuItemsView.Filter += MenuItems_Filter;
                _ingredientsView = new CollectionViewSource();
                _ingredientsView.Source = Ingredients;
                _ingredientsView.Filter += Ingredients_Filter;
                addItem = new Models.MenuItem();
                MenuItem = new Models.MenuItem();
                DishHasBeenAdded = false;
                AddItem.FoodImage = converting("pack://application:,,,/images/menu_default_image.jpg");
                EditView = Visibility.Visible;
                AddView = Visibility.Collapsed;
            # region Command executes
                AddDishes_Command = new RelayCommand<object>((p) => true, (p) =>
                {
                    AddView = Visibility.Visible;
                    EditView = Visibility.Collapsed;
                });
                SwitchToEditView_Command = new RelayCommand<object>((p) => true, (p) =>
                {
                    AddView = Visibility.Collapsed;
                    EditView = Visibility.Visible;
                });
                RemoveDish_Command = new RelayCommand<object>((p) =>
                {
                    if (MenuItem.FoodImage == null
                    || MenuItem.FoodName == ""
                    || MenuItem.ID == "") return false;
                    return true;
                }, (p) =>
                {
                    MyMessageBox msb = new MyMessageBox($"Việc xoá món ăn có thể dẫn đến mất mát dữ liệu của các phần khác. Bạn có muốn tiếp tục?", true);
                    msb.ShowDialog();
                    if (msb.ACCEPT() == true)
                    {
                        MenuDP.Flag.RemoveDish(MenuItem.ID);
                        MenuItems.Remove(MenuItem);
                        MyMessageBox msb2 = new MyMessageBox("Xoá thành công!");
                        msb2.Show();
                    }
                });
                AddDish_Command = new RelayCommand<Button>((p) =>
                {
                    if (AddItem.IsNullOrEmpty()
                    || IsListedInMenuList(AddItem.ID)) return false;
                    return true;
                }, (p) =>
                {
                    try
                    {
                        MenuDP.Flag.AddDish(AddItem);
                        MenuItems.Add(AddItem);
                        DishHasBeenAdded = true;
                        MyMessageBox msb = new MyMessageBox("Thêm thành công!");
                        msb.Show();
                    }
                    catch (SqlException e)
                    {
                        MyMessageBox msb = new MyMessageBox("Mã món không được trùng với \n mã món của các món đã tồn tại");
                        msb.Show();
                        return;
                    }
                });
                AddImage_Command = new RelayCommand<object>((p) => true, (p) =>
                {
                    OpenFileDialog op = new OpenFileDialog();
                    op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" + "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" + "Portable Network Graphic (*.png)|*.png";
                    op.Title = "Thêm ảnh món ăn";
                    if (op.ShowDialog() == DialogResult.OK)
                    {
                        BitmapImage bmi = new BitmapImage();
                        bmi.BeginInit();
                        bmi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bmi.CacheOption = BitmapCacheOption.OnLoad;
                        bmi.UriSource = new Uri(op.FileName);
                        bmi.EndInit();
                        AddItem.FoodImage = bmi;
                    }
                });
                SaveChanges_Command = new RelayCommand<object>((p) =>
                {
                    if (MenuItem.IsNullOrEmpty()) return false;
                    return true;
                }, (p) => {
                    try
                    {
                        MenuDP.Flag.EditDishInfo(MenuItem);
                        MyMessageBox msb = new MyMessageBox("Sửa thành công!");
                        msb.Show();
                    }
                    catch (Exception ex)
                    {
                        MyMessageBox msb = new MyMessageBox(ex.Message);
                        msb.Show();
                    }
                });
                DiscardChanges_Command = new RelayCommand<object>((p) => true, (p) => {
                    MenuItem = MenuDP.Flag.GetDishInfo(MenuItem.ID);
                });
                EditFoodImage_Command = new RelayCommand<object>((p) => true, (p) =>
                {
                    OpenFileDialog op = new OpenFileDialog();
                    op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" + "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" + "Portable Network Graphic (*.png)|*.png";
                    op.Title = "Đổi ảnh món ăn";
                    if (op.ShowDialog() == DialogResult.OK)
                    {
                        BitmapImage bmi = new BitmapImage();
                        bmi.BeginInit();
                        bmi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bmi.CacheOption = BitmapCacheOption.OnLoad;
                        bmi.UriSource = new Uri(op.FileName);
                        bmi.EndInit();
                        MenuItem.FoodImage = bmi;
                    }
                });
                AddIngredient_Command = new RelayCommand<object>((p) =>
                {
                    if (!DishHasBeenAdded) return false;
                    return true;
                }, (p) =>
                { 
                    MenuAdmin_ThemNLieu IngreAddView = new MenuAdmin_ThemNLieu();
                    IngreAddView.DataContext = this;
                    IngreAddView.ShowDialog();
                });
            #region Thêm nguyên liệu command execution
                AddIngredientsToDish_Command = new RelayCommand<object>((p) =>
                {
                    if (Selected_Ingredient == null) return false;
                    return true;
                }, (p) =>
                {
                    if (!IsListedInIngredientList(Selected_Ingredient.TenSanPham))
                    {
                        if (AddView == Visibility.Visible)
                        {
                            Ingredients_ForDishes.Add(new ChiTietMon(Selected_Ingredient.TenSanPham, AddItem.ID));  // can xem lai ve tinh logic , neu chua co ma mon
                        }
                        else if (EditView == Visibility.Visible)
                        {
                            Ingredients_ForDishes.Add(new ChiTietMon(Selected_Ingredient.TenSanPham, MenuItem.ID));
                        }
                    }
                    else
                    {
                        MyMessageBox msb = new MyMessageBox("Nguyên liệu này đã được thêm!");
                        msb.Show();
                    }
                });
                SaveDishIngredients_Command = new RelayCommand<object>((p) =>
                {
                    if (Ingredients_ForDishes.Count == 0) return false;
                    return true;
                }, (p) =>
                {
                    string message = "";
                    try
                    {
                        string foodName = "";                    

                        if (AddView == Visibility.Visible)
                        {
                            foodName = AddItem.FoodName;
                        }
                        else if (EditView == Visibility.Visible)
                        {
                            foodName = MenuItem.FoodName;
                        }
                        
                        if (Deleted_Ingredients.Count == 0)
                        {
                            if (CheckIfIngredientListInclude0InQuantity())
                            {
                                message = "Lượng nguyên liệu phải lớn hơn 0";
                            }
                            else
                            {
                                foreach (ChiTietMon ctm in Ingredients_ForDishes)
                                {
                                    MenuDP.Flag.SaveIngredients(ctm);
                                }
                                message = $"Đã thêm nguyên liệu cho món \n {foodName}";
                            }
                        }
                        else
                        {
                            if (CheckIfIngredientListInclude0InQuantity())
                            {
                                message = "Lượng nguyên liệu phải lớn hơn 0";
                            }
                            else
                            {
                                foreach (ChiTietMon ctm in Ingredients_ForDishes)
                                {
                                    MenuDP.Flag.SaveIngredients(ctm);
                                }
                            }
                            foreach (ChiTietMon ctm in Deleted_Ingredients)
                            {
                                MenuDP.Flag.RemoveIngredients(ctm);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        int n = 0;
                        foreach (ChiTietMon ctm in Ingredients_ForDishes)
                        {
                            n = MenuDP.Flag.UpdateIngredients(ctm);
                            if(n == 0)
                            {
                                MenuDP.Flag.SaveIngredients(ctm);
                            }
                            foreach (ChiTietMon x in Deleted_Ingredients)
                            {
                                MenuDP.Flag.RemoveIngredients(x);
                            }
                        }
                        message = $"Đã thêm và cập nhật nguyên liệu cho món {MenuItem.FoodName}";

                        return;
                    }
                    catch (Exception ex)
                    {
                        message = ex.Message;
                    }
                    finally
                    {
                        if (message == "")
                        {
                            message = "Lưu thành công!";
                        }
                        MyMessageBox msb = new MyMessageBox(message);
                        msb.Show();
                    }
                });
                HideIngredientWindow_Command = new RelayCommand<Window>((p) => true, (p) =>
                {
                    p.Close();
                    Deleted_Ingredients.Clear();
                });
                RemoveIngredientFromDish_Command = new RelayCommand<ChiTietMon>((p) => true, (p) =>
                {
                    Ingredients_ForDishes.Remove(p);
                    Deleted_Ingredients.Add(p);
                });
                EditIngredient_Command = new RelayCommand<object>((p) => true, (p) =>
                {
                    Ingredients_ForDishes = MenuDP.Flag.GetIngredientsForDish(MenuItem.ID);
                    MenuAdmin_ThemNLieu IngreAddView = new MenuAdmin_ThemNLieu();
                    IngreAddView.DataContext = this;
                    IngreAddView.ShowDialog();
                });

            #endregion
            #endregion
        }

        #region attributes
        private ObservableCollection<Models.MenuItem> _menuitems;
        private ObservableCollection<Models.Kho> _ingredients;
        private ObservableCollection<ChiTietMon> _ingredients_ForDishes;
        private ObservableCollection<ChiTietMon> _deletedIngredients;
        private string _filterText;
        private string _ingreFilterText;
        private CollectionViewSource _menuItemsView;
        private CollectionViewSource _ingredientsView;
        private Models.MenuItem _menuitem;
        private Models.Kho _selected_Ingredient;
        private Visibility editView;
        private Visibility addView;
        private Models.MenuItem addItem;
        private bool _dishHasBeenAdded;
        #endregion
        #region properties
        public ObservableCollection<Models.MenuItem> MenuItems { get { return _menuitems; } set { _menuitems = value; OnPropertyChanged(); } }
        public ObservableCollection<Models.Kho> Ingredients { get { return _ingredients; } set { _ingredients = value; OnPropertyChanged(); } }
        public ObservableCollection<ChiTietMon> Ingredients_ForDishes { get { return _ingredients_ForDishes;} set { _ingredients_ForDishes = value; OnPropertyChanged(); } }
        public ObservableCollection<ChiTietMon> Deleted_Ingredients { get { return _deletedIngredients; } set { _deletedIngredients = value; OnPropertyChanged(); } }
        public Models.MenuItem MenuItem { get { return _menuitem; } set { _menuitem = value; OnPropertyChanged(); } }
        public Models.MenuItem AddItem { get { return addItem; } set { addItem = value; OnPropertyChanged(); } }
        public Models.Kho Selected_Ingredient { get { return _selected_Ingredient; } set { _selected_Ingredient = value; OnPropertyChanged(); } }
        public bool DishHasBeenAdded { get { return _dishHasBeenAdded; } set { _dishHasBeenAdded = value; OnPropertyChanged(); } }
        public Visibility EditView { get { return editView; } set { editView = value; OnPropertyChanged(); } }
        public Visibility AddView { get { return addView; } set { addView = value; OnPropertyChanged(); } }
        public string FilterText { get { return _filterText; } set { _filterText = value; this._menuItemsView.View.Refresh(); OnPropertyChanged(); } }
        public string IngreFilterText { get { return _ingreFilterText; } set { _ingreFilterText = value; this._ingredientsView.View.Refresh(); OnPropertyChanged(); } }
        public ICollectionView MenuItemCollection
        {
            get
            {
                return this._menuItemsView.View;
            }
        }
        public ICollectionView IngredientCollection
        {
            get
            {
                return this._ingredientsView.View;
            }
        }
        #endregion
        #region commands
        public ICommand AddDishes_Command { get; set; }
        public ICommand AddDish_Command { get; set; }
        public ICommand SwitchToEditView_Command { get; set; }
        public ICommand RemoveDish_Command { get; set; }
        public ICommand AddImage_Command { get; set; }
        public ICommand SaveChanges_Command { get; set; }
        public ICommand DiscardChanges_Command { get; set; }
        public ICommand EditFoodImage_Command { get; set; }
        public ICommand AddIngredient_Command { get; set; }
        public ICommand EditIngredient_Command { get; set; }
        public ICommand AddIngredientsToDish_Command { get; set; }
        public ICommand RemoveIngredientFromDish_Command { get; set; }
        public ICommand SaveDishIngredients_Command { get; set; }
        public ICommand HideIngredientWindow_Command { get; set; }
        #endregion
        #region complementary functions
        public BitmapImage converting(string ur)
        {
            BitmapImage bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.CacheOption = BitmapCacheOption.OnLoad;
            bmi.UriSource = new Uri(ur);
            bmi.EndInit();

            return bmi;
        }
        public bool IsListedInIngredientList(string TenNL)
        {
            if (Ingredients_ForDishes.Count == 0) return false;
            foreach(ChiTietMon ctm in Ingredients_ForDishes)
            {
                if(ctm.TenNL.CompareTo(TenNL) == 0)
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsListedInMenuList(string MaMon)
        {
            if (MenuItems.Count == 0) return false;
            foreach(Models.MenuItem mi in MenuItems)
            {
                if (string.Compare(mi.ID, MaMon) == 0)
                    return true;
            }
            return false;
        }
        public bool CheckIfIngredientListInclude0InQuantity()
        {
            foreach(ChiTietMon ctm in Ingredients_ForDishes)
            {
                if(ctm.SoLuong <= 0)
                {
                    return true;
                }
            }
            return false;
        }
        public void MenuItems_Filter(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrEmpty(FilterText))
            {
                e.Accepted = true;
                return;
            }

            Models.MenuItem item = e.Item as Models.MenuItem;
            if (item.FoodName.RemoveDiacritics().ToLower().Contains(FilterText.RemoveDiacritics().ToLower()))
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = false;
            }
        }
        private void Ingredients_Filter(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrEmpty(IngreFilterText))
            {
                e.Accepted = true;
                return;
            }

            Models.Kho item = e.Item as Models.Kho;
            if (item.TenSanPham.RemoveDiacritics().ToLower().Contains(IngreFilterText.RemoveDiacritics().ToLower()))
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = false;
            }
        }
        private async Task LoadMenu()
        {
            _menuitems = await MenuDP.Flag.ConvertToCollection();
        }
        #endregion
    }
}
