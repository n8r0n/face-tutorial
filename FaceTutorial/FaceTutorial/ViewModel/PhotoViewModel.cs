using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Convolution;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Drawing.Brushes;
using SixLabors.ImageSharp.Processing.Drawing.Pens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FaceTutorial.ViewModel
{
    public class PhotoViewModel : ViewModelBase
    {
        // Replace the first parameter with your valid subscription key.
        //
        // Replace or verify the region in the second parameter.
        //
        // You must use the same region in your REST API call as you used to obtain your subscription keys.
        // For example, if you obtained your subscription keys from the westus region, replace
        // "westcentralus" in the URI below with "westus".
        //
        // NOTE: Free trial subscription keys are generated in the westcentralus region, so if you are using
        // a free trial subscription key, you should not need to change this region.
        private readonly IFaceServiceClient faceServiceClient =
           //new FaceServiceClient("e8a66ec58e52465884d728233c11efbf", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");
           new FaceServiceClient("ff0840bd35ca4326aaeebc592ca583a8", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");

        private Face[] faces;                   // The list of detected faces.
        private String[] faceDescriptions;      // The list of descriptions for the detected faces.
        private double resizeFactor;            // The resize factor for the displayed image.

        public PhotoViewModel()
        {
            BlurFacesCommand = new RelayCommand<object>(OnBlurFacesCommandAsync, CanBlurFacesCommand);
        }

        public ICommand BlurFacesCommand { get; private set; }

        private async void OnBlurFacesCommandAsync(object obj)
        {
            string filePath = (string)obj;

            Uri fileUri = new Uri(filePath);
            BitmapImage bitmapSource = new BitmapImage();
            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.None;
            bitmapSource.UriSource = fileUri;
            bitmapSource.EndInit();

            PhotoSource = bitmapSource;

            // Detect any faces in the image.
            Title = "Detecting...";
            faces = await UploadAndDetectFaces(filePath);
            Title = String.Format("Detection Finished. {0} face(s) detected", faces.Length);

            FaceRectangle[] faceRectangles = new FaceRectangle[faces.Length];
            for (int i = 0; i < faces.Length; i++)
            {
                faceRectangles[i] = faces[i].FaceRectangle;
            }
            // blur faces and also draw a rectangle around each face.
            BlurFaces(faceRectangles, filePath);
        }

        private bool CanBlurFacesCommand(object obj)
        {
            // Maybe you would check for a valid path here, or use some other
            //  logic to determine if the command is available (now)
            return (obj is string && obj != null);
        }

        private ImageSource _photoSource;
        public ImageSource PhotoSource
        {
            get
            {
                return _photoSource;
            }
            set
            {
                _photoSource = value;
                RaisePropertyChanged("PhotoSource");
            }
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            private set
            {
                _title = value;
                RaisePropertyChanged("Title");
            }
        }


        // Uploads the image file and calls Detect Faces.
        private async Task<Face[]> UploadAndDetectFaces(string imageFilePath)
        {
            // The list of Face attributes to return.
            IEnumerable<FaceAttributeType> faceAttributes =
                new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Smile, FaceAttributeType.Emotion, FaceAttributeType.Glasses, FaceAttributeType.Hair };

            // Call the Face API.
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    Face[] faces = await faceServiceClient.DetectAsync(imageFileStream, returnFaceId: true, returnFaceLandmarks: false, returnFaceAttributes: faceAttributes);
                    return faces;
                }
            }
            // Catch and display Face API errors.
            catch (FaceAPIException f)
            {
                MessageBox.Show(f.ErrorMessage, f.ErrorCode);
                return new Face[0];
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
                return new Face[0];
            }
        }

        private void BlurFaces(FaceRectangle[] faceRects, string sourceImage)
        {
            Image<Rgba32> image;
            using (FileStream stream = File.OpenRead(sourceImage))
            {
                image = SixLabors.ImageSharp.Image.Load(stream);
                IPen<Rgba32> pen = new Pen<Rgba32>(new SolidBrush<Rgba32>(Rgba32.Red), 2);
                foreach (var faceRect in faceRects)
                {
                    var rectangle = new SixLabors.Primitives.Rectangle(
                        faceRect.Left,
                        faceRect.Top,
                        faceRect.Width,
                        faceRect.Height);
                    image.Mutate(img =>
                    {
                        img.BoxBlur<Rgba32>(20, rectangle);
                        img.Draw(pen, rectangle);
                    });
                }
            }

            // stackoverflow.com/questions/5782913/how-to-convert-from-type-image-to-type-bitmapimage
            MemoryStream memoryStream = new MemoryStream();
            image.Save(memoryStream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            //bitmap.CacheOption = BitmapCacheOption.OnLoad;
            // bitmap.UriSource = null;
            bitmap.StreamSource = memoryStream;
            bitmap.EndInit();

            PhotoSource = bitmap;
        }
    }
}
