using ScrewTurn.Wiki.Configuration;
using ScrewTurn.Wiki.Web.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using ScrewTurn.Wiki.PluginFramework;
using ScrewTurn.Wiki.Web.Localization.Install;
using ScrewTurn.Wiki.Web.Localization.Messages;

namespace ScrewTurn.Wiki.Web.Controllers
{
    public class InstallController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        public ApplicationSettings Settings { get; private set; }

        private IConfigReaderWriter _configReaderWriter { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallController"/> class.
        /// </summary>
        /// <param name="settings"></param>
        public InstallController(ApplicationSettings settings, IConfigReaderWriter configReaderWriter)
        {
            Settings = settings;
            _configReaderWriter = configReaderWriter;
        }

        /// <summary>
        /// Returns Javascript 'constants' for the installer.
        /// </summary>
        public ActionResult InstallerJsVars()
        {
            if (Settings.Installed && !Settings.NeedMasterPassword)
                return Content("");

            return View();
        }

        /// <summary>
        /// Displays the language choice page.
        /// </summary>
        public ActionResult Index()
        {
            if (Settings.Installed && !Settings.NeedMasterPassword)
                return RedirectToAction("Index", "Home");

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

            return View("Index", LanguageViewModel.SupportedLocales());
        }

        /// <summary>
        /// Displays the start page for the installer (step1).
        /// </summary>
        public ActionResult Step1(string language)
        {
            if (Settings.Installed && !Settings.NeedMasterPassword)
                return RedirectToAction("Index", "Home");

            SetLanguage(language);

            return View(new InstallViewModel() { LanguageCode = language });
        }

        /// <summary>
        /// Displays the DB settings (step2).
        /// </summary>
        public ActionResult Step2(InstallViewModel model)
        {
            if (Settings.Installed && !Settings.NeedMasterPassword)
                return RedirectToAction("Index", "Home");

            SetLanguage(model.LanguageCode);

            return View(model);
        }

        /// <summary>
        /// Displays the wikis (step3).
        /// </summary>
        public ActionResult Step3(InstallViewModel model)
        {
            if (Settings.Installed && !Settings.NeedMasterPassword)
                return RedirectToAction("Index", "Home");

            SetLanguage(model.LanguageCode);

            if (model.Wikis.Count == 0)
                model.Wikis.Add("root", "");

            return View(model);
        }

        /// <summary>
		/// Validates the POST'd <see cref="InstallViewModel"/> object. If the settings are valid,
		/// an attempt is made to install using this.
        /// </summary>
        public ActionResult Step4(InstallViewModel model)
        {
            if (Settings.Installed && !Settings.NeedMasterPassword)
                return RedirectToAction("Index", "Home");

            SetLanguage(model.LanguageCode);
            model.Installed = Settings.Installed;
            model.NeedMasterPassword = Settings.NeedMasterPassword;

            if (!Settings.Installed)
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var settings = MappingToApplicationSettings(model);
                        _configReaderWriter.Save(settings);
                    }
                }
                catch (Exception e)
                {
                    try
                    {
                        _configReaderWriter.ResetInstalledState();
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(InstallStrings.FailureInstall, ex.Message + e);
                    }
                    ModelState.AddModelError(InstallStrings.FailureInstall, e.Message + e);
                }
            }

            return View(model);
        }

        [HttpGet]
        public JsonResult CreateMasterPassword(string password)
        {
            if (!Settings.Installed || !Settings.NeedMasterPassword)
                return Json("");

            string errors = "";
            try
            {
                GlobalSettings.SetMasterPassword(Hash.Compute(password));
            }
            catch (Exception ex)
            {
                Log.LogEntry("Setup master password has error: " + ExceptionHelper.GetAllExceptionMessages(ex), EntryType.Error, Log.SystemUsername, null);
                errors = ExceptionHelper.GetLastInnerExceptionMessage(ex);
            }
            return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);

            //lblResult.Text = Properties.Messages.ConfigSaved;
        }

        /// <summary>
        /// WebApp shutdown
        /// </summary>
        /// <returns></returns>
        public ActionResult FinalizeInstall()
        {
            if (Settings.Installed && !Settings.NeedMasterPassword)
            {
                Log.LogEntry("WebApp shutdown requested", EntryType.General, SessionFacade.CurrentUsername, null);
                Response.Clear();
                Response.Write(String.Format(Messages.WebApplicationShutdown, "Default") + "\n\n");
                Response.Flush();
                Response.Close();
                Log.LogEntry("Executing WebApp shutdown", EntryType.General, Log.SystemUsername, null);
                HttpRuntime.UnloadAppDomain();
            }
            return new EmptyResult();
        }

        private ApplicationSettings MappingToApplicationSettings(InstallViewModel model)
        {
            var settings = new ApplicationSettings();

            settings.PublicDirectory = model.PublicDirectory;
            settings.Wikis = new List<Configuration.Wiki>();
            foreach (var wiki in model.Wikis)
            {
                var hosts = wiki.Value?.Split(';').ToList() ?? new List<string>();
                settings.Wikis.Add(new Configuration.Wiki(wiki.Key, hosts));
            }

            if (!model.AdvancedSettings)
            {
                var scheme = ConnectionScheme.AllSchemes.First(t => t.Name == model.ConnectionSchemeName);
                if (scheme == null)
                    throw new Exception("The connection scheme is not defined");
                settings.GlobalSettingsStorageProvider = scheme.GlobalSettingsStorageProvider;
                settings.GlobalSettingsStorageProviderConfig = model.ConnectionString;
                settings.FilesProviders = new List<StorageProvider>() { MapToStorageProvider(scheme.FilesProvider, model.ConnectionString, true) };
                settings.IndexDirectoryProvider = MapToStorageProvider(scheme.IndexDirectoryProvider, model.ConnectionString, true);
                settings.PagesProviders = new List<StorageProvider>() { MapToStorageProvider(scheme.PagesProvider, model.ConnectionString, true) };
                settings.SettingProvider =  MapToStorageProvider(scheme.SettingProvider, model.ConnectionString, true);
                settings.ThemesProviders = new List<StorageProvider>() { MapToStorageProvider(scheme.ThemesProvider, model.ConnectionString, true) };
                settings.UsersProviders = new List<StorageProvider>() { MapToStorageProvider(scheme.UsersProvider, model.ConnectionString, true) };
            }
            settings.Installed = true;

            return settings;
        }

        private StorageProvider MapToStorageProvider(string providerName, string connectionName, bool isDefault)
        {
            var provider = providerName.Split(',');
            return new StorageProvider() {AssemblyName = provider[1].Trim(), ConfigurationString = connectionName, IsDefault = isDefault, TypeName = provider[0].Trim()};
        }

        private void SetLanguage(string language)
        {
            if (!string.IsNullOrEmpty(language))
            {
                if (LanguageViewModel.SupportedLocales().First(x => x.Code == language) != null)
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            }
        }
    }
}