using InventoryManagement.Data;
using System.Windows;

namespace InventoryManagement
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            using (var db = new InventoryDbContext())
            {
                db.Database.EnsureCreated();
            }
        }
    }
}