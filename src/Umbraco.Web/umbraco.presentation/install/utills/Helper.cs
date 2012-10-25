using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;

namespace umbraco.presentation.install {
  public static class Helper {

    public static void RedirectToNextStep(Page page) {
      _default d = (_default)page;
      d.GotoNextStep(d.step.Value);
    }

    public static void RedirectToLastStep(Page page)
    {
        _default d = (_default)page;
        d.GotoLastStep();
    }


    private static int m_Percentage = -1;
    public static int Percentage { get { return m_Percentage; } set { m_Percentage = value; } }
    
    public static string Description { get; set; }
    public static string Error { get; set; }


    public static void clearProgress()
    {
        Percentage = -1;
        Description = string.Empty;
        Error = string.Empty;
    }

    public static void setProgress(int percent, string description, string error)
    {
        if (percent > 0)
            Percentage = percent;

       Description = description;
       Error = error;
    }

    public static string getProgress(){
        ProgressResult pr = new ProgressResult(Percentage, Description, Error);
        JavaScriptSerializer js = new JavaScriptSerializer();
        return js.Serialize(pr);
    }
  }

    public class ProgressResult
    {
        public string Error { get; set; }
        public int Percentage { get; set; }
        public string Description { get; set; }
        public ProgressResult()
        {
            
        }

        public ProgressResult(int percentage, string description, string error)
        {
            Percentage = percentage;
            Description = description;
            Error = error;
        }

    }
}