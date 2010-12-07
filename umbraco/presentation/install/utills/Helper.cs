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

    public static int Percentage { get; set; }
    public static string Description { get; set; }
    public static string Error { get; set; }


    public static void setSession(string alias, int percent, string description, string error)
    {
        if (percent > 0)
            Percentage = percent;


       Description = description;
       Error = error;
    }

    public static string getProgress(){
        string json = @"{'progress': {
                                        'percentage': '%p',
                                        'message': '%m',
                                        'error': '%e'
                                        }   
                                 }";
        string retval = json.Replace("%p", Percentage.ToString()).Replace("%m", Description).Replace("%e", Error);
        return retval;
    }
  }
}