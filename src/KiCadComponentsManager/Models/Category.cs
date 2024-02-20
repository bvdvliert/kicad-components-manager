using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiCadComponentsManager.Models
{
    public class Category
    {
        public ObservableCollection<Category>? SubCategories { get; }
        public string Title { get; }

        public Category(string title)
        {
            Title = title;
        }

        public Category(string title, ObservableCollection<Category> subCategories)
        {
            Title = title;
            SubCategories = subCategories;
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
