using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using NUnit.Framework;
using ScrewTurn.Wiki.Web;
using ScrewTurn.Wiki.Web.Controllers;
using ScrewTurn.Wiki.Configuration;

namespace ScrewTurn.Wiki.Web.Tests.Controllers
{
    [TestFixture]
    public class HomeControllerTest
    {
        [Test]
        public void Index()
        {
            var settings = new ApplicationSettings();
            // Arrange
            WikiController controller = new WikiController(settings);

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }


        //[Test]
        //public void TestPrivateMethod()
        //{
        //    var settings = new ApplicationSettings();
        //    // Arrange
        //    HomeController controller = new HomeController(settings);
        //    PrivateObject privateObject = new PrivateObject(controller);

        //    //privateObject.SetField("field", "Don't panic");
        //    privateObject.Invoke("PrintField");
        //}

        //[Test]
        //public void About()
        //{
        //    var settings = new ApplicationSettings();
        //    // Arrange
        //    HomeController controller = new HomeController(settings);

        //    // Act
        //    ViewResult result = controller.About() as ViewResult;

        //    // Assert
        //    Assert.AreEqual("Your application description page.", result.ViewBag.Message);
        //}

        //[Test]
        //public void Contact()
        //{
        //    var settings = new ApplicationSettings();
        //    // Arrange
        //    HomeController controller = new HomeController(settings);

        //    // Act
        //    ViewResult result = controller.Contact() as ViewResult;

        //    // Assert
        //    Assert.IsNotNull(result);
        //}
    }
}
