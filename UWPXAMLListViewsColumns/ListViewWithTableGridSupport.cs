using System.Collections;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace UWPXAMLListViewsColumns
{
    public class ListViewWithTableGridSupport : ListView
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ListViewWithTableGridSupportAutomationPeer(this);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            // Provide a way for the cell's AutomationProviders to 
            // know their ContaingGrid, and what row they're in.
            MyGridData data = item as MyGridData;
            if (data != null)
            {
                ListViewItem lvItem = element as ListViewItem;
                if (lvItem != null)
                {
                    AutomationProperties.SetLocalizedControlType(lvItem, "Row");

                    ListView listView = ItemsControl.ItemsControlFromItemContainer(lvItem) as ListView;

                    data.ContainingGrid = listView;
                    data.RowIndex = listView.IndexFromContainer(lvItem);
                }
            }
        }
    }

    public class ListViewWithTableGridSupportAutomationPeer : 
        ListViewAutomationPeer, IGridProvider, ITableProvider
    {
        private ListViewWithTableGridSupport owner;

        public ListViewWithTableGridSupportAutomationPeer(ListViewWithTableGridSupport owner) : base(owner)
        {
            this.owner = owner;
        }

        public IRawElementProviderSimple GetREPS()
        {
            return ProviderFromPeer(this);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            // UIA says that list's mustn't support the Table pattern.
            return AutomationControlType.DataGrid;
        }

        protected override object GetPatternCore(PatternInterface patternInterface)
        {
            if ((patternInterface == PatternInterface.Grid) ||
                (patternInterface == PatternInterface.Table))
            {
                return this;
            }

            return base.GetPatternCore(patternInterface);
        }

        // A couple of static helper functions.

        static public FrameworkElement GetDescendantFromName(DependencyObject parent, string name)
        {
            FrameworkElement element = null;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                element = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
                if (element != null)
                {
                    if (element.Name == name)
                    {
                        return element;
                    }

                    element = GetDescendantFromName(element, name);
                    if (element != null)
                    {
                        return element;
                    }
                }
            }

            return element;
        }

        static public FrameworkElement GetAncestorFromName(DependencyObject child, string name)
        {
            var element = child as FrameworkElement;

            while (element != null)
            {
                if (element.Name == name)
                {
                    return element;
                }

                element = VisualTreeHelper.GetParent(element) as FrameworkElement;
            }

            return element;
        }

        // Grid pattern implementation.

        public IRawElementProviderSimple GetItem(int row, int column)
        {
            IRawElementProviderSimple reps = null;

            if (row <= owner.Items.Count)
            {
                DependencyObject depObj = null;

                // Row 0 is the header row.
                if (row == 0)
                {
                    depObj = owner.Header as DependencyObject;
                }
                else
                {
                    depObj = owner.ContainerFromItem(owner.Items[row - 1]) as ListViewItem;
                }

                if (depObj != null)
                {
                    var cell = GetDescendantFromName(depObj, "Cell" + column);
                    if (cell != null)
                    {
                        var peer = FrameworkElementAutomationPeer.FromElement(cell) as
                                        UserControlWithTableItemGridItemSupportAutomationPeer;
                        if (peer != null)
                        {
                            reps = peer.GetREPS();
                        }
                    }
                }
            }

            return reps;
        }

        public int ColumnCount
        {
            get
            {
                var header = owner.Header as DependencyObject;

                int cellIndex = 0;
                while (GetDescendantFromName(header, "Cell" + cellIndex) != null)
                {
                    ++cellIndex;
                }

                return cellIndex;
            }
        }

        public int RowCount
        {
            get
            {
                // Include the header row in the row count.
                return owner.Items.Count + 1;
            }
        }

        // Table pattern implementation.

        // Barker Todo: Figure out why AIWin reports no headers being returned from GetColumnHeaders,
        // despite it listing the headers just fine if the same code is run beneath GetRowHeaders.

        public IRawElementProviderSimple[] GetColumnHeaders()
        {
            var repsArray = new ArrayList();

            var header = owner.Header as DependencyObject;

            FrameworkElement headerCell;
            int cellIndex = 0;
            while ((headerCell = GetDescendantFromName(header, "Cell" + cellIndex)) != null)
            {
                ++cellIndex;

                var peer = FrameworkElementAutomationPeer.FromElement(headerCell) as
                                UserControlWithTableItemGridItemSupportAutomationPeer;
                if (peer != null)
                {
                    repsArray.Add(peer.GetREPS());
                }
            }

            IRawElementProviderSimple[] result = null;

            if (repsArray.Count > 0)
            {
                result = repsArray.ToArray(repsArray[0].GetType()) as IRawElementProviderSimple[];
            }

            return result;
        }

        public IRawElementProviderSimple[] GetRowHeaders()
        {
            // This demo contains no row headers.
            return new IRawElementProviderSimple[0];
        }

        public RowOrColumnMajor RowOrColumnMajor
        {
            get
            {
                // The data in this example is primarily read by row.
                return RowOrColumnMajor.RowMajor;
            }
        }
    }

    // Can't derive from the sealed TextBlock, so use a UserControl for the cell
    // and add support for the GridItem and TableItem patterns to it.

    public class UserControlWithTableItemGridItemSupport : UserControl
    {
        private ListViewWithTableGridSupport myListView;
        public ListViewWithTableGridSupport MyListView { get => myListView; set => myListView = value; }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new UserControlWithTableItemGridItemSupportAutomationPeer(this);
        }
    }

    public class UserControlWithTableItemGridItemSupportAutomationPeer :
        FrameworkElementAutomationPeer, IGridItemProvider, ITableItemProvider
    {
        private UserControlWithTableItemGridItemSupport owner;

        public UserControlWithTableItemGridItemSupportAutomationPeer(UserControlWithTableItemGridItemSupport owner) : base(owner)
        {
            this.owner = owner;
        }

        public IRawElementProviderSimple GetREPS()
        {
            return ProviderFromPeer(this);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return (this.Row == 0 ? AutomationControlType.HeaderItem : AutomationControlType.DataItem);
        }

        protected override IList<AutomationPeer> GetChildrenCore()
        {
            // Hide the contained TextBlock, because its text is already
            // being exposed as the Name of the containing UserControl.
            return new List<AutomationPeer>();
        }

        protected override object GetPatternCore(PatternInterface patternInterface)
        {
            if ((patternInterface == PatternInterface.GridItem) ||
                (patternInterface == PatternInterface.TableItem))
            {
                return this;
            }

            return base.GetPatternCore(patternInterface);
        }

        // GridItem pattern implementation.

        public int Row
        {
            get
            {
                int row = 0;

                // The row index is based on the ListView, not the grid within 
                // the ListView.Header/ItemTemplate's DataTemplate.
                var myData = owner.DataContext as MyGridData;
                if (myData != null)
                {
                    row = myData.RowIndex + 1;
                }
                else
                {
                    // This item is on the header row.
                    row = 0;
                }

                return row;
            }
        }

        public int RowSpan
        {
            get
            {
                // This demo code assumes a RowSpan of 1.
                return 1;
            }
        }

        public int Column
        {
            get
            {
                // It's sufficient for this demo to get the column index from 
                // the ListView.Header/ItemTemplate's DataTemplate directly.
                return Grid.GetColumn(owner);
            }
        }

        public int ColumnSpan
        {
            get
            {
                // This demo code assumes a ColumnSpan of 1.
                return 1;
            }
        }

        public IRawElementProviderSimple ContainingGrid
        {
            get
            {
                UIElement lvElement = null;

                var myData = owner.DataContext as MyGridData;
                if (myData != null)
                {
                    lvElement = myData.ContainingGrid;
                }
                else
                {
                    lvElement = ListViewWithTableGridSupportAutomationPeer.GetAncestorFromName(
                                    owner, "ExperimentalListView") as ListViewWithTableGridSupport;
                }

                IRawElementProviderSimple reps = null;

                var peer = FrameworkElementAutomationPeer.FromElement(lvElement) as
                                    ListViewWithTableGridSupportAutomationPeer;
                if (peer != null)
                {
                    reps = peer.GetREPS();
                }

                return reps;
            }
        }

        // TableItem pattern implementation.

        public IRawElementProviderSimple[] GetColumnHeaderItems()
        {
            IRawElementProviderSimple[] result = null;

            var repsArray = new ArrayList();

            var myData = owner.DataContext as MyGridData;
            if (myData != null)
            {
                var header = myData.ContainingGrid.Header as DependencyObject;

                FrameworkElement headerCell = ListViewWithTableGridSupportAutomationPeer.GetDescendantFromName(
                    header, "Cell" + this.Column);

                var peer = FrameworkElementAutomationPeer.FromElement(headerCell) as
                                UserControlWithTableItemGridItemSupportAutomationPeer;
                if (peer != null)
                {
                    repsArray.Add(peer.GetREPS());
                }

                if (repsArray.Count > 0)
                {
                    result = repsArray.ToArray(repsArray[0].GetType()) as IRawElementProviderSimple[];
                }
            }

            return result;
        }

        public IRawElementProviderSimple[] GetRowHeaderItems()
        {
            // This demo UI has no row headers.
            return new IRawElementProviderSimple[0];
        }
    }
}
