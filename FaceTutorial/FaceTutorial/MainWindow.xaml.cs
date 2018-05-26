using FaceTutorial.ViewModel;
using Microsoft.ProjectOxford.Face.Contract;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FaceTutorial
{
    public partial class MainWindow : Window
    {
        private PhotoViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = (PhotoViewModel)DataContext;
        }

        ///<summary> Displays the image and calls Detect Faces. </summary>
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the image file to scan from the user.
            var openDlg = new Microsoft.Win32.OpenFileDialog();

            openDlg.Filter = viewModel.ImageFileTypes;

            // Only do something if user picked a file
            if ((bool)openDlg.ShowDialog(this))
            {
                // Display and blur faces in the image file.
                string filePath = openDlg.FileName;

                if (viewModel.BlurFacesCommand.CanExecute(filePath))
                {
                    viewModel.BlurFacesCommand.Execute(filePath);
                }
            }
        }

        ///<summary> Displays the face description when the mouse is over a face rectangle. </summary>
        private void FacePhoto_MouseMove(object sender, MouseEventArgs e)
        {
            // If the REST call has not completed, return from this method.
            if (viewModel.Faces == null)
                return;

            // Find the mouse position relative to the image.
            System.Windows.Point mouseXY = e.GetPosition(FacePhoto);

            BitmapSource bitmapSource = (BitmapSource)viewModel.PhotoSource;
            double dpi = bitmapSource.DpiX;
            double resizeFactor = 96 / dpi;
            // Scale adjustment between the actual size and displayed size.
            var scale = FacePhoto.ActualWidth / (bitmapSource.PixelWidth / resizeFactor);

            // Check if this mouse position is over a face rectangle.
            bool mouseOverFace = false;
            for (int i = 0; i < viewModel.Faces.Length; ++i)
            {
                FaceRectangle fr = viewModel.Faces[i].FaceRectangle;
                double left = fr.Left * scale;
                double top = fr.Top * scale;
                double width = fr.Width * scale;
                double height = fr.Height * scale;

                // Display the face description for this face if the mouse is over this face rectangle.
                if (mouseXY.X >= left && mouseXY.X <= left + width && mouseXY.Y >= top && mouseXY.Y <= top + height)
                {
                    faceDescriptionStatusBar.Text = viewModel.FaceDescriptions[i];
                    mouseOverFace = true;
                    break;
                }
            }

            // If the mouse is not over a face rectangle.
            if (!mouseOverFace)
                faceDescriptionStatusBar.Text = "Place the mouse pointer over a face to see the face description.";
        }
    }
}