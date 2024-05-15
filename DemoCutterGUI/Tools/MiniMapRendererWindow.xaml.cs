using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DemoCutterGUI.Tools
{
    /// <summary>
    /// Interaction logic for MiniMapRendererWindow.xaml
    /// </summary>
    public partial class MiniMapRendererWindow : Window
    {
        public MiniMapRendererWindow()
        {
            InitializeComponent();
            InitMiniMap();
        }
        internal MiniMapRenderer miniMapRenderer { get; private set; } = null;

        public event EventHandler RangeAppliedButtonPressed;
        public event EventHandler MiniMapPointEdited;

        private void OnRangeAppliedButtonPressed(EventArgs e)
        {
            RangeAppliedButtonPressed?.Invoke(this, e);
        }
        private void OnMiniMapPointEdited(EventArgs e)
        {
            MiniMapPointEdited?.Invoke(this, e);
        }

        void InitMiniMap()
        {
            miniMapRenderer = new MiniMapRenderer(OpenTkControl,true);

        }

        private void resetViewBtn_Click(object sender, RoutedEventArgs e)
        {
            miniMapRenderer.ResetView();
        }

        private void applyRangeBtn_Click(object sender, RoutedEventArgs e)
        {
            OnRangeAppliedButtonPressed(new EventArgs());
        }

        MiniMapPointLogical miniMapPointEditorPoint = null;
        public void editMiniMapPointNote(MiniMapPointLogical position)
        {
            miniMapPointEditorPoint = position;
            Dispatcher.Invoke(() => {
                miniMapPointEditor.Visibility = Visibility.Visible;
                miniMapPointEditorNoteTxt.Text = miniMapPointEditorPoint.note ?? "";
            });
        }

        private void miniMapPointEditorOkBtn_Click(object sender, RoutedEventArgs e)
        {
            MiniMapPointLogical position = miniMapPointEditorPoint;
            miniMapPointEditorPoint = null;
            miniMapPointEditor.Visibility = Visibility.Collapsed;
            string editedNote = miniMapPointEditorNoteTxt.Text;
            miniMapPointEditorNoteTxt.Text = "";
            if (position is null) return;
            position.note = editedNote;
            OnMiniMapPointEdited(new EventArgs());
        }
    }
}
