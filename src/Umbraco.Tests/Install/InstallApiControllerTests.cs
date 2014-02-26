using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Web.Install.Controllers;
using Umbraco.Web.Install.Models;

namespace Umbraco.Tests.Install
{
    [TestFixture]
    public class InstallApiControllerTests
    {

        //[Test]
        //public void Can_Execute_Step()
        //{
        //    var step = new InstallSetupStep<DatabaseModel>()
        //    {
        //        Name = "Database",
        //        View = "database",
        //        ServerOrder = 0,
        //        RequiresRestart = true,
        //        ExecuteCallback = model => LogHelper.Info<InstallApiControllerTests>("installing....")
        //    };

        //    var instruction = JObject.FromObject(new
        //    {
        //        dbType = 0,
        //        server = "localhost",
        //        databaseName = "test",
        //        login = "sa",
        //        password = "test",
        //        integratedAuth = false,
        //        connectionString = string.Empty
        //    });

        //    Assert.DoesNotThrow(() => InstallApiController.ExecuteStep(step, instruction));

        //}

    }
}
