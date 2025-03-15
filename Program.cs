using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InventoryManagementSystem
{
    // Product class with required properties
    public class Product
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public override string ToString()
        {
            return $"ID: {ID}, Name: {Name}, Price: ${Price:F2}, Stock: {Stock}";
        }
    }

    // Receipt item class to track items in a sale
    public class ReceiptItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total => UnitPrice * Quantity;
    }

    // Receipt class to generate receipts
    public class Receipt
    {
        public int ReceiptId { get; set; }
        public DateTime Date { get; set; }
        public List<ReceiptItem> Items { get; set; }
        public decimal Total => Items.Sum(item => item.Total);

        public Receipt()
        {
            Date = DateTime.Now;
            Items = new List<ReceiptItem>();
        }

        public string GenerateReceiptText()
        {
            var sb = new StringBuilder();

            sb.AppendLine("===========================================");
            sb.AppendLine("          INVENTORY MANAGEMENT SYSTEM      ");
            sb.AppendLine("===========================================");
            sb.AppendLine($"Receipt #: {ReceiptId}");
            sb.AppendLine($"Date: {Date.ToString("MM/dd/yyyy HH:mm:ss")}");
            sb.AppendLine("-------------------------------------------");
            sb.AppendLine("Qty  Item                     Price   Total");
            sb.AppendLine("-------------------------------------------");

            foreach (var item in Items)
            {
                sb.AppendLine($"{item.Quantity,3}  {item.ProductName,-23} ${item.UnitPrice,5:F2}  ${item.Total,6:F2}");
            }

            sb.AppendLine("-------------------------------------------");
            sb.AppendLine($"TOTAL:                               ${Total,6:F2}");
            sb.AppendLine("===========================================");
            sb.AppendLine("          Thank you for your purchase!     ");
            sb.AppendLine("===========================================");

            return sb.ToString();
        }
    }

    // Inventory Manager to handle CRUD operations
    public class InventoryManager
    {
        private Dictionary<int, Product> _products;
        private int _nextId = 1;
        private int _nextReceiptId = 1001;

        public InventoryManager()
        {
            _products = new Dictionary<int, Product>();
        }

        // Create: Add a new product
        public int AddProduct(string name, decimal price, int initialStock)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty");

            if (price <= 0)
                throw new ArgumentException("Price must be greater than zero");

            if (initialStock < 0)
                throw new ArgumentException("Initial stock cannot be negative");

            // Create new product with auto-generated ID
            int id = _nextId++;
            var product = new Product
            {
                ID = id,
                Name = name,
                Price = price,
                Stock = initialStock
            };

            // Add to dictionary
            _products.Add(id, product);
            return id;
        }

        // Read: Get a specific product
        public Product GetProduct(int id)
        {
            if (!_products.ContainsKey(id))
                throw new KeyNotFoundException($"Product with ID {id} not found");

            return _products[id];
        }

        // Read: Get all products
        public List<Product> GetAllProducts()
        {
            return _products.Values.ToList();
        }

        // Update: Update product information
        public void UpdateProduct(int id, string name, decimal price)
        {
            if (!_products.ContainsKey(id))
                throw new KeyNotFoundException($"Product with ID {id} not found");

            if (!string.IsNullOrWhiteSpace(name))
                _products[id].Name = name;

            if (price > 0)
                _products[id].Price = price;
        }

        // Update: Update stock level
        public void UpdateStock(int id, int newStock)
        {
            if (!_products.ContainsKey(id))
                throw new KeyNotFoundException($"Product with ID {id} not found");

            if (newStock < 0)
                throw new ArgumentException("Stock cannot be negative");

            _products[id].Stock = newStock;
        }

        // Create a new receipt for a sale with multiple items
        public Receipt CreateSale()
        {
            var receipt = new Receipt { ReceiptId = _nextReceiptId++ };
            return receipt;
        }

        // Add item to receipt and update inventory
        public void AddItemToSale(Receipt receipt, int productId, int quantity)
        {
            if (!_products.ContainsKey(productId))
                throw new KeyNotFoundException($"Product with ID {productId} not found");

            var product = _products[productId];

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");

            if (product.Stock < quantity)
                throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");

            // Reduce stock
            product.Stock -= quantity;

            // Add to receipt
            receipt.Items.Add(new ReceiptItem
            {
                ProductId = productId,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity
            });
        }

        // Delete: Remove a product
        public void RemoveProduct(int id)
        {
            if (!_products.ContainsKey(id))
                throw new KeyNotFoundException($"Product with ID {id} not found");

            _products.Remove(id);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var inventory = new InventoryManager();
            bool running = true;

            while (running)
            {
                Console.Clear();
                Console.WriteLine("===== Inventory Management System =====");
                Console.WriteLine("1. View all products");
                Console.WriteLine("2. Add new product");
                Console.WriteLine("3. Update product details");
                Console.WriteLine("4. Update stock level");
                Console.WriteLine("5. Create a sale (with receipt)");
                Console.WriteLine("6. Remove product");
                Console.WriteLine("0. Exit");
                Console.Write("\nEnter your choice: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine();

                    switch (choice)
                    {
                        case 1:
                            DisplayProducts(inventory);
                            break;

                        case 2:
                            AddProduct(inventory);
                            break;

                        case 3:
                            UpdateProduct(inventory);
                            break;

                        case 4:
                            UpdateStock(inventory);
                            break;

                        case 5:
                            ProcessSaleWithReceipt(inventory);
                            break;

                        case 6:
                            RemoveProduct(inventory);
                            break;

                        case 0:
                            running = false;
                            break;

                        default:
                            Console.WriteLine("Invalid choice.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                }

                if (running)
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                }
            }

            Console.WriteLine("Thank you for using the Inventory Management System!");
        }

        static void DisplayProducts(InventoryManager inventory)
        {
            var products = inventory.GetAllProducts();

            if (products.Count == 0)
            {
                Console.WriteLine("No products in inventory.");
                return;
            }

            Console.WriteLine("Product List:");
            Console.WriteLine("------------");

            foreach (var product in products)
            {
                Console.WriteLine(product);
            }
        }

        static void AddProduct(InventoryManager inventory)
        {
            Console.Write("Enter product name: ");
            string name = Console.ReadLine();

            Console.Write("Enter price: $");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price))
            {
                Console.WriteLine("Invalid price.");
                return;
            }

            Console.Write("Enter initial stock: ");
            if (!int.TryParse(Console.ReadLine(), out int stock))
            {
                Console.WriteLine("Invalid stock quantity.");
                return;
            }

            try
            {
                int id = inventory.AddProduct(name, price, stock);
                Console.WriteLine($"Product added successfully with ID: {id}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void UpdateProduct(InventoryManager inventory)
        {
            DisplayProducts(inventory);

            Console.Write("\nEnter product ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID.");
                return;
            }

            try
            {
                // Display current info
                var product = inventory.GetProduct(id);
                Console.WriteLine($"Current details: {product}");

                Console.Write("Enter new name (leave empty to keep current): ");
                string name = Console.ReadLine();

                Console.Write("Enter new price (0 to keep current): $");
                decimal price = 0;
                decimal.TryParse(Console.ReadLine(), out price);

                inventory.UpdateProduct(id, name, price);
                Console.WriteLine("Product updated successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void UpdateStock(InventoryManager inventory)
        {
            DisplayProducts(inventory);

            Console.Write("\nEnter product ID to update stock: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID.");
                return;
            }

            Console.Write("Enter new stock level: ");
            if (!int.TryParse(Console.ReadLine(), out int stock))
            {
                Console.WriteLine("Invalid stock quantity.");
                return;
            }

            try
            {
                inventory.UpdateStock(id, stock);
                Console.WriteLine("Stock updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void ProcessSaleWithReceipt(InventoryManager inventory)
        {
            try
            {
                // Create a new receipt
                var receipt = inventory.CreateSale();
                bool addingItems = true;

                while (addingItems)
                {
                    DisplayProducts(inventory);

                    Console.Write("\nEnter product ID to sell (0 to finish): ");
                    if (!int.TryParse(Console.ReadLine(), out int id) || id < 0)
                    {
                        Console.WriteLine("Invalid ID.");
                        continue;
                    }

                    if (id == 0)
                    {
                        addingItems = false;
                        continue;
                    }

                    Console.Write("Enter quantity to sell: ");
                    if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
                    {
                        Console.WriteLine("Invalid quantity.");
                        continue;
                    }

                    try
                    {
                        inventory.AddItemToSale(receipt, id, quantity);
                        Console.WriteLine("Item added to sale.");

                        Console.Write("Add another item? (Y/N): ");
                        string response = Console.ReadLine();
                        if (response.ToUpper() != "Y")
                        {
                            addingItems = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }

                if (receipt.Items.Count > 0)
                {
                    // Print receipt
                    Console.Clear();
                    string receiptText = receipt.GenerateReceiptText();
                    Console.WriteLine(receiptText);

                    // Option to save receipt to file
                    Console.Write("\nDo you want to save this receipt to a file? (Y/N): ");
                    string saveOption = Console.ReadLine();
                    if (saveOption.ToUpper() == "Y")
                    {
                        string filename = $"Receipt_{receipt.ReceiptId}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                        System.IO.File.WriteAllText(filename, receiptText);
                        Console.WriteLine($"Receipt saved to {filename}");
                    }
                }
                else
                {
                    Console.WriteLine("Sale cancelled - no items added.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing sale: {ex.Message}");
            }
        }

        static void RemoveProduct(InventoryManager inventory)
        {
            DisplayProducts(inventory);

            Console.Write("\nEnter product ID to remove: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID.");
                return;
            }

            try
            {
                inventory.RemoveProduct(id);
                Console.WriteLine("Product removed successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}