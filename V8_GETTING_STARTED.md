## A quick start guide for getting up and running with Umbraco v8

### What you need

* [Visual Studio 2017 Community (Free)](https://www.visualstudio.com/vs/community/), or Professional, Enterprise, etc..
* .NET Framework 4.7.2 installed
    * get it here: https://www.microsoft.com/net/download/thank-you/net472?survey=false
* .NET Framework 4.7.2 developer pack
    * get it here: https://www.microsoft.com/net/download/thank-you/net472-developer-pack _(be sure this is the ENU file which will be named `NDP472-DevPack-ENU.exe`)_
* Clone the Umbraco repository and ensure you have the `temp8` branch checked out.

### Start the solution

* Open the `/src/umbraco.sln` Visual Studio solution.
* Start the solution (the easiest way is to use `ctrl + F5`)
  * Note: it may take some time on the first build as it will restore all nuget, npm and bower packages, build the .NET solution and the Angular solution.
* When the website starts you'll see the Umbraco installer. Just follow the prompts.
* You're all set!

### Making code changes

* _The process for making code changes in v8 is the same as v7_.
* Any .NET changes you make just need to be compiled.
* For any Angular/JS changes you make you will need to ensure you are running the Gulp build. The easiest way to do this is from within Visual Studio, in the `Task Runner Explorer`. You can find this window by pressing `ctrl + q` and typing in `Task Runner Explorer`. In this window you'll see all Gulp tasks; double click on the `dev` task. This will compile the Angular solution and start a file watcher. Any HTML/JS changes you make are automatically built.
  * When making JS changes, you should have the Chrome developer tools open to ensure that cache is disabled.

### What to work on

We are keeping track of [known issues and limitations here](http://issues.umbraco.org/issue/U4-11279). These line items will eventually be turned into actual tasks to be worked on. Feel free to help us keep this list updated if you find issues and even help fix some of these items. If there is a particular item you'd like to help fix please mention this on the task and we'll create a sub task for the item to continue discussion there.

There is [a list of tasks for v8 that haven't been completed](http://issues.umbraco.org/issues/U4?q=Due+in+version%3A+8.0.0+%23Unresolved+). If you are interested in helping out with any of these please mention this on the task. This list will be constantly updated as we begin to document and design some of the other tasks that still need to get done.
