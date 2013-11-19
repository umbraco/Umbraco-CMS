using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.cms.businesslogic.web;
using Umbraco.Tests.BusinessLogic;

namespace Umbraco.Tests
{

    //
    // new db
    //
    //10:44:43 PM: 1. started
    //10:44:43 PM: 2. init
    //10:44:55 PM: 3. ensure test data
    //10:44:55 PM: 4. tear down
    //10:44:55 PM: 5. fixture down
    //10:44:55 PM: 6. finished


    //
    // not new db
    //
    //10:45:46 PM: 1. started
    //10:45:46 PM: 2. init
    //10:45:47 PM: 3. ensure test data
    //10:45:47 PM: 4. tear down
    //10:45:47 PM: 5. fixture down
    //10:45:47 PM: 6. finished

    class Program
    {
        protected static void l(string format, params object[] args)
        {
            string m = string.Format(format, args);
            m = string.Format("{0:T}: ", DateTime.Now) + m;
            System.Console.WriteLine(m); 
        }
        public static void Main(string[] args)
        {
            try
            {
                //var o = new cms_businesslogic_ContentTypeTests();

                //o.Initialize();

                //o.GetAll_ReturnsAllContentTypes();

                //o.TearDown();

                //o.FixtureTearDown();

            }
            catch (Exception ex)
            {
                System.Console.WriteLine("*** ERROR = '{0}' ***", ex.Message);
            }
            finally
            {
                System.Console.WriteLine("Finished.");  
            }
        }
    }
}


//l("1. started");
//var o = new cms_businesslogic_TaskType_Tests(); // cms_businesslogic_A_Test();

//l("2. init");
//o.Initialize();

//l("3. ensure test data");
////o.Test_EnsureData(); 
//o._1st_Test_EnsureTestData();

//l("4. tear down");
//o.TearDown();

//l("5. fixture down");
//o.FixtureTearDown();
