using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace umbraco.presentation.install {
  public static class Helper {

    public static void RedirectToNextStep(Page page) {
      _default d = (_default)page;
      d.GotoNextStep(d.step.Value);
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
        string json = @"{
                            'percentage': '%p',
                            'message': '%m',
                            'error': '%e'
                        }";
        string retval = json.Replace("%p", Percentage.ToString()).Replace("%m", Description).Replace("%e", Error).Replace("'", "\"");
        return retval;
    }
  }
}