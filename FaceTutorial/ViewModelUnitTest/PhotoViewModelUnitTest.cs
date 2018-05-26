using System;
using System.Threading;
using FaceTutorial.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ViewModelUnitTest
{
    [TestClass]
    public class PhotoViewModelUnitTest
    {
        /// <summary>
        /// This is the location on the tester's machine with a JPG file containing faces.
        /// May need to update code below if the number of faces is not correct.
        /// </summary>
        private static string PhotoFilePath;

        [ClassInitialize]
        public static void TestClassInitialize(TestContext context)
        {
            // This variable is defined in the UnitTest.runsettings file. Each tester should change this for their environment:
            PhotoFilePath = context.Properties["FacesPhotoFilePath"].ToString();
        }

        [TestMethod]
        public void TestMethod1()
        {
            // This test really doesn't do anything useful
            int a = 6;
            int b = 12;
            Assert.AreEqual(2, b / a);
        }

        [TestMethod]
        public void TestUploadAndDetectFaces()
        {
            // Create a view model and ask it to detect and blur faces
            PhotoViewModel vm = new PhotoViewModel();
            bool canExecute = vm.BlurFacesCommand.CanExecute(PhotoFilePath);
            Assert.IsTrue(canExecute, "photo file path not valid");

            vm.BlurFacesCommand.Execute(PhotoFilePath);

            // TODO: fix this sleep !
            Thread.Sleep(10000);

            Assert.IsNotNull(vm.Faces, "no faces were found");
            Assert.IsTrue(vm.Faces.Length == 6, "the correct number of faces were not detected");
        }
    }
}
