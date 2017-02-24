using System.Web;
using System.Web.Hosting;
using System.Web.Optimization;

namespace ScrewTurnWiki.Web
{
    public class BundleConfig
    {
        public const string ModernizrJS = "~/bundles/modernizr";
        public const string JqueryUnobtrusiveJS = "~/bundles/jquery.unobtrusive-ajax";
        public const string JqueryJS = "~/bundles/jquery";
        public const string JqueryUIJS = "~/bundles/jquery-ui";
        public const string JqueryUICss = "~/Content/jquery/jquery-ui-css";
        //public const string JqueryvalJS = "~/bundles/jqueryval";
        public const string BootstrapJS = "~/bundles/bootstrap";
        public const string BootstrapCss = "~/Content/bootstrapcss";
        public const string RespondJS = "~/bundles/respond";
        public const string BootboxJS = "~/bundles/bootbox";

        public const string NotyJS = "~/bundles/noty";

        public const string AdminCss = "~/Content/Themes/Admin";
        public const string EditorCss = "~/Content/Themes/Editor";
        public const string JqueryEasingJS = "~/bundles/Admin/jquery.easing";

        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //BundleTable.VirtualPathProvider = new ScriptBundlePathProvider(HostingEnvironment.VirtualPathProvider);

#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif

            bundles.Add(new ScriptBundle(JqueryJS).Include("~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle(JqueryUnobtrusiveJS).Include("~/Scripts/jquery.unobtrusive-ajax.js"));

            bundles.Add(new ScriptBundle(JqueryUIJS).Include("~/Scripts/jquery-ui.js"));
            bundles.Add(new StyleBundle(JqueryUICss).Include("~/Content/jquery/css/jquery-ui*"));

            //bundles.Add(new ScriptBundle(JqueryvalJS).Include(
            //            "~/Scripts/jquery.unobtrusive*",
            //            "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle(ModernizrJS).Include("~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle(RespondJS).Include("~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle(BootstrapJS).Include("~/Content/bootstrap/js/bootstrap.js"));

            bundles.Add(new StyleBundle(BootstrapCss).Include("~/Content/bootstrap/css/bootstrap.css"));

            bundles.Add(new ScriptBundle(BootboxJS).Include("~/Scripts/bootbox.js"));

            bundles.Add(new ScriptBundle(NotyJS).Include("~/Scripts/noty/jquery.noty.packaged.js"));

            bundles.Add(new StyleBundle(AdminCss).Include("~/Content/Themes/Admin.css"));
            bundles.Add(new StyleBundle(EditorCss).Include("~/Content/Themes/Editor.css"));
            bundles.Add(new ScriptBundle(JqueryEasingJS).Include("~/Scripts/stw/admin/jquery.easing.1.3.js"));
        }
    }




    //class ScriptBundlePathProvider : VirtualPathProvider
    //{
    //    private readonly VirtualPathProvider _virtualPathProvider;

    //    public ScriptBundlePathProvider(VirtualPathProvider virtualPathProvider)
    //    {
    //        _virtualPathProvider = virtualPathProvider;
    //    }

    //    public override bool FileExists(string virtualPath)
    //    {
    //        return _virtualPathProvider.FileExists(virtualPath);
    //    }

    //    public override VirtualFile GetFile(string virtualPath)
    //    {
    //        return _virtualPathProvider.GetFile(virtualPath);
    //    }

    //    public override VirtualDirectory GetDirectory(string virtualDir)
    //    {
    //        return _virtualPathProvider.GetDirectory(virtualDir);
    //    }

    //    public override bool DirectoryExists(string virtualDir)
    //    {
    //        return _virtualPathProvider.DirectoryExists(virtualDir);
    //    }
    //}
}
