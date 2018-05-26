using System;
using System.Threading;
using FaceTutorial.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ViewModelUnitTest
{
    [TestClass]
    public class PhotoViewModelUnitTest
    {
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
            bool canExecute = vm.BlurFacesCommand.CanExecute("C:\\Users\\nscan\\Desktop\\faces.jpg");  // TODO: hardcoded file path
            Assert.IsTrue(canExecute);

            vm.BlurFacesCommand.Execute("C:\\Users\\nscan\\Desktop\\faces.jpg");

            // TODO: fix this sleep !
            Thread.Sleep(10000);

            Assert.IsNotNull(vm.Faces);
            Assert.IsTrue(vm.Faces.Length == 6);
        }
    }
}
