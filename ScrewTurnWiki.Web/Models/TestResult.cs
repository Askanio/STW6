using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScrewTurn.Wiki.Web.Models
{
    /// <summary>
    /// Basic error information for the JSON actions
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// Any error message associated with the call.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Indicates if there are any errors.
        /// </summary>
        public bool Success
        {
            get { return string.IsNullOrEmpty(ErrorMessage); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestResult"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public TestResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}