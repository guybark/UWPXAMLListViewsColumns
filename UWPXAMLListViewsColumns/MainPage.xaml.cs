using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;

namespace UWPXAMLListViewsColumns
{
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<MyGridData> MyData;

        public MainPage()
        {
            InitializeComponent();

            this.MyData = new ObservableCollection<MyGridData>();
            this.DataContext = MyData;

            this.MyData.Add(new MyGridData() { Name = "Wallace", ModifiedDate = "1/1/20" });
            this.MyData.Add(new MyGridData() { Name = "Gromit", ModifiedDate = "2/2/20" });
            this.MyData.Add(new MyGridData() { Name = "Cheese", ModifiedDate = "3/3/20" });
        }
    }

    public class MyGridData
    {
        private string name;
        private string modifiedDate;
        private int rowIndex;
        private ListView containingGrid;

        public override string ToString()
        {
            return Name + ", " + ModifiedDate;
        }

        public string Name { get => name; set => name = value; }
        public string ModifiedDate { get => modifiedDate; set => modifiedDate = value; }
        public int RowIndex { get => rowIndex; set => rowIndex = value; }
        public ListView ContainingGrid { get => containingGrid; set => containingGrid = value; }
    }
}
