using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using InventoryManagement.Data;
using InventoryManagement.Models;

namespace InventoryManagement
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            using (var db = new InventoryDbContext())
            {
                CategoryListBox.ItemsSource = db.Categories.ToList();
            }
        }

        private void LoadProducts()
        {
            var selectedCategory = CategoryListBox.SelectedItem as Category;
            if (selectedCategory == null) return;

            using (var db = new InventoryDbContext())
            {
                var products = db.Products
                    .Where(p => p.CategoryId == selectedCategory.Id)
                    .ToList();
                ProductsListBox.ItemsSource = products;
            }
        }

        private void LoadMovements()
        {
            var selectedProduct = ProductsListBox.SelectedItem as Product;
            if (selectedProduct == null) return;

            using (var db = new InventoryDbContext())
            {
                var movements = db.StockMovements
                    .Where(m => m.ProductId == selectedProduct.Id)
                    .OrderByDescending(m => m.Date)
                    .ToList();
                MovementsListBox.ItemsSource = movements;
            }
        }

        private void CategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProducts();
        }

        private void ProductsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadMovements();
        }

        private void AddCategoryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CategoryNameBox.Text)) return;

            using (var db = new InventoryDbContext())
            {
                db.Categories.Add(new Category { Name = CategoryNameBox.Text });
                db.SaveChanges();
            }

            CategoryNameBox.Clear();
            LoadCategories();
        }

        private void AddProductBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedCategory = CategoryListBox.SelectedItem as Category;
            if (selectedCategory == null || string.IsNullOrWhiteSpace(ProductNameBox.Text)) return;

            int.TryParse(ProductQuantityBox.Text, out int quantity);

            using (var db = new InventoryDbContext())
            {
                db.Products.Add(new Product
                {
                    Name = ProductNameBox.Text,
                    SKU = ProductSKUBox.Text,
                    Quantity = quantity,
                    CategoryId = selectedCategory.Id
                });
                db.SaveChanges();
            }

            ProductNameBox.Clear();
            ProductSKUBox.Clear();
            ProductQuantityBox.Clear();
            LoadProducts();
        }

        private void AddMovementBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedProduct = ProductsListBox.SelectedItem as Product;
            if (selectedProduct == null) return;

            if (!int.TryParse(MovementQuantityBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество!");
                return;
            }

            bool isIn = RadioIn.IsChecked == true;

            using (var db = new InventoryDbContext())
            {
                var product = db.Products.Find(selectedProduct.Id);
                if (product != null)
                {
                    if (!isIn && product.Quantity < quantity)
                    {
                        MessageBox.Show("Недостаточно товара!");
                        return;
                    }

                    product.Quantity += isIn ? quantity : -quantity;

                    db.StockMovements.Add(new StockMovement
                    {
                        ProductId = selectedProduct.Id,
                        Type = isIn ? MovementType.In : MovementType.Out,
                        Quantity = quantity,
                        Date = DateTime.Now,
                        Comment = MovementCommentBox.Text
                    });

                    db.SaveChanges();
                    MessageBox.Show($"Остаток: {product.Quantity} шт.");
                }
            }

            MovementQuantityBox.Clear();
            MovementCommentBox.Clear();
            LoadProducts();
            LoadMovements();
        }
    }
}