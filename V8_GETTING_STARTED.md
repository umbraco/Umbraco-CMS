## A quick start guide for getting up and runnning with Umbraco v8

### What you need:

* [Visual Studio 2017 Community (Free)](https://www.visualstudio.com/vs/community/), or Professional, Enterprise, etc... 
* .NET Framework 4.7.2 installed, get it here: https://www.microsoft.com/net/download/thank-you/net472?survey=false
* .NET Framework 4.7.2 developer pack, get it here: https://www.microsoft.com/net/download/thank-you/net472-developer-pack _(be sure this is the ENU file which will be named `NDP472-DevPack-ENU.exe`)_
* Clone the Umbraco repository and ensure you have the `temp8` branch checked out

### Start the solution

* Open the `/src/umbraco.sln` Visual Studio solution
* Start the solution (easiest way is to use `ctrl + F5`)
  * When the solution is first built this may take some time since it will restore all nuget, npm and bower packages, build the .net solution and also build the angular solution.
* When the website starts you'll see the Umbraco installer and just follow the prompts
* Your all set!

### Making code changes

* Any .NET changes you make you just need to compile
* Any Angular/JS changes you make you will need to make sure you are running the Gulp build. Easiest way to do this is from within Visual Studio in the `Task Runner Explorer`. You can find this window by pressing `ctrl + q` and typing in `Task Runner Explorer`. In this window you'll see all Gulp tasks, double click on the `dev` task, this will compile the angular solution and start a file watcher, then any html/js changes you make are automatically built.  
  * When making js changes, you should have the chrome developer tools open to ensure that cache is disabled
