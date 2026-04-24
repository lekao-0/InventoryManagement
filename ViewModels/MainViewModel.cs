using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using InventoryManagement.Data;
using InventoryManagement.Models;

namespace InventoryManagement.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private ObservableCollection<Category> _categories;
        private ObservableCollection<Product> _products;
        private ObservableCollection<StockMovement> _movements;
        
        private Category _selectedCategory;
        private Product _selectedProduct;
        
        private string _newProductName;
        private string _newProductSKU;
        private string _newProductQuantity = "0";
        private string _newCategoryName;
        
        private MovementType _selectedMovementType = MovementType.In;
        private string _movementQuantity = "0";
        private string _movementComment;

        public ICommand AddCategoryCommand { get; private set; }
        public ICommand AddProductCommand { get; private set; }
        public ICommand DeleteProductCommand { get; private set; }
        public ICommand AddMovementCommand { get; private set; }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged("Categories");
            }
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged("Products");
            }
        }

        public ObservableCollection<StockMovement> Movements
        {
            get => _movements;
            set
            {
                _movements = value;
                OnPropertyChanged("Movements");
            }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged("SelectedCategory");
                if (value != null)
                    LoadProducts();
            }
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged("SelectedProduct");
                if (value != null)
                    LoadMovements();
            }
        }

        public string NewProductName
        {
            get => _newProductName;
            set
            {
                _newProductName = value;
                OnPropertyChanged("NewProductName");
            }
        }

        public string NewProductSKU
        {
            get => _newProductSKU;
            set
            {
                _newProductSKU = value;
                OnPropertyChanged("NewProductSKU");
            }
        }

        public string NewProductQuantity
        {
            get => _newProductQuantity;
            set
            {
                _newProductQuantity = value;
                OnPropertyChanged("NewProductQuantity");
            }
        }

        public string NewCategoryName
        {
            get => _newCategoryName;
            set
            {
                _newCategoryName = value;
                OnPropertyChanged("NewCategoryName");
            }
        }

        public MovementType SelectedMovementType
        {
            get => _selectedMovementType;
            set
            {
                _selectedMovementType = value;
                OnPropertyChanged("SelectedMovementType");
            }
        }

        public string MovementQuantity
        {
            get => _movementQuantity;
            set
            {
                _movementQuantity = value;
                OnPropertyChanged("MovementQuantity");
            }
        }

        public string MovementComment
        {
            get => _movementComment;
            set
            {
                _movementComment = value;
                OnPropertyChanged("MovementComment");
            }
        }

        public MainViewModel()
        {
            AddCategoryCommand = new RelayCommand(AddCategoryExecute);
            AddProductCommand = new RelayCommand(AddProductExecute);
            DeleteProductCommand = new RelayCommand(DeleteProductExecute);
            AddMovementCommand = new RelayCommand(AddMovementExecute);
            
            LoadCategories();
        }

        private void AddCategoryExecute(object parameter)
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName)) 
                return;

            try
            {
                using (var db = new InventoryDbContext())
                {
                    db.Categories.Add(new Category { Name = NewCategoryName });
                    db.SaveChanges();
                }
                
                NewCategoryName = string.Empty;
                LoadCategories();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при добавлении категории: {ex.Message}");
            }
        }

        private void AddProductExecute(object parameter)
        {
            if (SelectedCategory == null || string.IsNullOrWhiteSpace(NewProductName)) 
                return;

            if (!int.TryParse(NewProductQuantity, out int quantity))
            {
                System.Windows.MessageBox.Show("Введите корректное количество!");
                return;
            }

            try
            {
                using (var db = new InventoryDbContext())
                {
                    var product = new Product
                    {
                        Name = NewProductName,
                        SKU = NewProductSKU,
                        Quantity = quantity,
                        CategoryId = SelectedCategory.Id
                    };
                    db.Products.Add(product);
                    db.SaveChanges();
                }
                
                NewProductName = string.Empty;
                NewProductSKU = string.Empty;
                NewProductQuantity = "0";
                LoadProducts();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при добавлении товара: {ex.Message}");
            }
        }

        private void DeleteProductExecute(object parameter)
        {
            if (SelectedProduct == null) 
                return;

            try
            {
                using (var db = new InventoryDbContext())
                {
                    var product = db.Products.Find(SelectedProduct.Id);
                    if (product != null)
                    {
                        db.Products.Remove(product);
                        db.SaveChanges();
                    }
                }
                
                LoadProducts();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при удалении товара: {ex.Message}");
            }
        }

        private void AddMovementExecute(object parameter)
        {
            if (SelectedProduct == null)
            {
                System.Windows.MessageBox.Show("Выберите товар!");
                return;
            }

            if (!int.TryParse(MovementQuantity, out int quantity) || quantity <= 0)
            {
                System.Windows.MessageBox.Show("Введите корректное количество!");
                return;
            }

            try
            {
                using (var db = new InventoryDbContext())
                {
                    var product = db.Products.Find(SelectedProduct.Id);
                    if (product != null)
                    {
                        if (SelectedMovementType == MovementType.Out && product.Quantity < quantity)
                        {
                            System.Windows.MessageBox.Show("Недостаточно товара на складе!");
                            return;
                        }

                        if (SelectedMovementType == MovementType.In)
                            product.Quantity += quantity;
                        else
                            product.Quantity -= quantity;

                        db.StockMovements.Add(new StockMovement
                        {
                            ProductId = SelectedProduct.Id,
                            Type = SelectedMovementType,
                            Quantity = quantity,
                            Date = DateTime.Now,
                            Comment = MovementComment
                        });
                        
                        db.SaveChanges();
                    }
                }
                
                MovementQuantity = "0";
                MovementComment = string.Empty;
                LoadProducts();
                LoadMovements();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при выполнении операции: {ex.Message}");
            }
        }

        private void LoadCategories()
        {
            try
            {
                using (var db = new InventoryDbContext())
                {
                    var list = db.Categories.ToList();
                    Categories = new ObservableCollection<Category>(list);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}");
            }
        }

        private void LoadProducts()
        {
            if (SelectedCategory == null) 
                return;
            
            try
            {
                using (var db = new InventoryDbContext())
                {
                    var list = db.Products
                        .Where(p => p.CategoryId == SelectedCategory.Id)
                        .ToList();
                    Products = new ObservableCollection<Product>(list);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке товаров: {ex.Message}");
            }
        }

        private void LoadMovements()
        {
            if (SelectedProduct == null) 
                return;
            
            try
            {
                using (var db = new InventoryDbContext())
                {
                    var list = db.StockMovements
                        .Where(m => m.ProductId == SelectedProduct.Id)
                        .OrderByDescending(m => m.Date)
                        .ToList();
                    Movements = new ObservableCollection<StockMovement>(list);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке истории: {ex.Message}");
            }
        }
    }
}