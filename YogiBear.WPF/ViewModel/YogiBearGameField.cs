using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using YogiBear.Persistence;

namespace YogiBear.WPF.ViewModel
{
    public class YogiBearGameField : ViewModelBase
    {
        private Brush? backgroundColor;
        private Pieces? content;

        public int X { get; set; }
        public int Y { get; set; }

        public Pieces? Content
        {
            get => content;
            set
            {
                content = value;
                OnPropertyChanged();
            }
        }

        public Brush? BackgroundColor
        {
            get => backgroundColor;
            set
            {
                backgroundColor = value;
                OnPropertyChanged();
            }
        }
    }
}
