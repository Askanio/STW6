using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrewTurn.Wiki
{
    /// <summary>
    /// 
    /// </summary>
    public static class ExceptionHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetLastInnerExceptionMessage(Exception ex)
        {
            if (ex.InnerException != null)
                return GetLastInnerExceptionMessage(ex);
            return ex.Message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetAllExceptionMessages(Exception ex)
        {
            var result = ex.Message;
            if (ex.InnerException != null)
                result = $"{result}; {GetAllExceptionMessages(ex.InnerException)}";
            return result ?? "";
        }

        /// <summary>
        /// Format all InnerException
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="headerMessage"></param>
        public static string BuildLogError(Exception ex, string headerMessage)
        {
            var sb = new StringBuilder();
            sb.AppendLine(headerMessage);
            BuildLogError(ex, ref sb);
            return sb.ToString();
        }

        /// <summary>
        /// Format all InnerException
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="sb"></param>
        private static void BuildLogError(Exception ex, ref StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendFormat("{0} thrown {1}", ex.Source, ex.GetType().FullName).AppendLine();
            sb.AppendLine(ex.Message);
            sb.AppendLine(ex.StackTrace);
            if (ex.InnerException != null)
                BuildLogError(ex.InnerException, ref sb);
        }
    }
}
