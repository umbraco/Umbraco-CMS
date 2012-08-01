using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;

namespace umbraco.controls
{
    public class GenericValidation : IValidator
    {
        
            public static void AddValidationError(Page page, string message)
            {
                page.Validators.Add(new GenericValidation(message));
            }

            private GenericValidation(string message)
            {
                ErrorMessage = message;
                IsValid = false;
            }

            public string ErrorMessage { get; set; }

            public bool IsValid { get; set; }

            public void Validate()
            {
            }


    }
}
