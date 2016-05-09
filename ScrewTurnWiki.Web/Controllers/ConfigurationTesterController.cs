using System;
using System.Web.Mvc;
using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Models;

namespace ScrewTurn.Wiki.Web.Controllers
{
    public class ConfigurationTesterController : Controller
    {
        private ApplicationSettings _settings { get; set; }

        private IConfigReaderWriter _configReaderWriter { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallController"/> class.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="configReaderWriter"></param>
        public ConfigurationTesterController(ApplicationSettings settings, IConfigReaderWriter configReaderWriter)
        {
            _settings = settings;
            _configReaderWriter = configReaderWriter;
        }

        /// <summary>
        /// Attempts to write to the web.config file and save it.
        /// </summary>
        /// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
        public JsonResult TestWebConfig()
        {
            if (!AllowTesting())
                return Json("");

            string errors = _configReaderWriter.TestSaveWebConfig();
            return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
        }

        internal bool AllowTesting()
        {
            return !_settings.Installed;// TODO:|| _userContext.IsAdmin;
        }

        /// <summary>
        /// Checks exists the public pirectory and attempts to save same file to it.
        /// </summary>
        /// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
        public JsonResult TestPublicDirectory(string publicDirectory)
        {
            if (!AllowTesting())
                return Json("");

            string errors = _configReaderWriter.TestPublicDirectory(publicDirectory);
            return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Attempts a database connection using the provided connection string.
        /// </summary>
        /// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
        public JsonResult TestDatabaseConnection(string connectionString, string connectionSchemeName)
        {
            if (!AllowTesting())
                return Json("");

            string errors = null;
            try
            {
                foreach (var scheme in ConnectionScheme.AllSchemes)
                {
                    if (scheme.Name == connectionSchemeName)
                    {
                        IProviderV60 globalSettingsStorageProvider = ProviderLoader.LoadGlobalSettingsStorageProvider(scheme.GlobalSettingsStorageProvider);
                        globalSettingsStorageProvider.ValidateConfig(connectionString);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                errors = ExceptionHelper.GetAllExceptionMessages(ex);
            }

            return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
        }
    }
}