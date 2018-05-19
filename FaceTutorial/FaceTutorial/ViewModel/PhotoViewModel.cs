using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.ProjectOxford.Common.Contract;
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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FaceTutorial.ViewModel
{
    public class PhotoViewModel : ViewModelBase
    {
        #region Class Members
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
        #endregion

        public PhotoViewModel()
        {
            BlurFacesCommand = new RelayCommand<object>(OnBlurFacesCommandAsync, CanBlurFacesCommand);
        }

        #region Commands

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
            Faces = await UploadAndDetectFaces(filePath);
            Title = String.Format("Detection Finished. {0} face(s) detected", Faces.Length);

            FaceRectangle[] faceRectangles = new FaceRectangle[Faces.Length];
            FaceDescriptions = new string[Faces.Length];
            for (int i = 0; i < Faces.Length; i++)
            {
                faceRectangles[i] = Faces[i].FaceRectangle;
                FaceDescriptions[i] = FaceDescription(Faces[i]);
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

        #endregion

        #region Properties

        ///<summary> The list of descriptions for the detected faces. </summary>
        public String[] FaceDescriptions { get; private set; }

        ///<summary> The list of detected faces. </summary>
        public Face[] Faces { get; private set; }                

        private ImageSource _photoSource;
        /// <summary> The source data for the View's photo. </summary>
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
        /// <summary> A title to display app name or status. </summary>
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
        #endregion

        #region Private API methods

        ///<summary> Uploads the image file and calls Detect Faces. </summary>
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

        /// <summary>
        /// Blur faces inside the given rectangular areas.
        /// </summary>
        /// <param name="faceRects">Rectangles defining the areas to be blurred.</param>
        /// <param name="sourceImage">The original image to blur.</param>
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

        ///<summary> Returns a string that describes the given face. </summary>
        private string FaceDescription(Face face)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Face: ");

            // Add the gender, age, and smile.
            sb.Append(face.FaceAttributes.Gender);
            sb.Append(", ");
            sb.Append(face.FaceAttributes.Age);
            sb.Append(", ");
            sb.Append(String.Format("smile {0:F1}%, ", face.FaceAttributes.Smile * 100));

            // Add the emotions. Display all emotions over 10%.
            sb.Append("Emotion: ");
            EmotionScores emotionScores = face.FaceAttributes.Emotion;
            if (emotionScores.Anger >= 0.1f) sb.Append(String.Format("anger {0:F1}%, ", emotionScores.Anger * 100));
            if (emotionScores.Contempt >= 0.1f) sb.Append(String.Format("contempt {0:F1}%, ", emotionScores.Contempt * 100));
            if (emotionScores.Disgust >= 0.1f) sb.Append(String.Format("disgust {0:F1}%, ", emotionScores.Disgust * 100));
            if (emotionScores.Fear >= 0.1f) sb.Append(String.Format("fear {0:F1}%, ", emotionScores.Fear * 100));
            if (emotionScores.Happiness >= 0.1f) sb.Append(String.Format("happiness {0:F1}%, ", emotionScores.Happiness * 100));
            if (emotionScores.Neutral >= 0.1f) sb.Append(String.Format("neutral {0:F1}%, ", emotionScores.Neutral * 100));
            if (emotionScores.Sadness >= 0.1f) sb.Append(String.Format("sadness {0:F1}%, ", emotionScores.Sadness * 100));
            if (emotionScores.Surprise >= 0.1f) sb.Append(String.Format("surprise {0:F1}%, ", emotionScores.Surprise * 100));

            // Add glasses.
            sb.Append(face.FaceAttributes.Glasses);
            sb.Append(", ");

            // Add hair.
            sb.Append("Hair: ");

            // Display baldness confidence if over 1%.
            if (face.FaceAttributes.Hair.Bald >= 0.01f)
                sb.Append(String.Format("bald {0:F1}% ", face.FaceAttributes.Hair.Bald * 100));

            // Display all hair color attributes over 10%.
            HairColor[] hairColors = face.FaceAttributes.Hair.HairColor;
            foreach (HairColor hairColor in hairColors)
            {
                if (hairColor.Confidence >= 0.1f)
                {
                    sb.Append(hairColor.Color.ToString());
                    sb.Append(String.Format(" {0:F1}% ", hairColor.Confidence * 100));
                }
            }

            // Return the built string.
            return sb.ToString();
        }
        #endregion
    }
}
