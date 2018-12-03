## A quick start guide for getting up and runnning with Umbraco v8

### What you need:

* [Visual Studio 2017 Community (Free)](https://www.visualstudio.com/vs/community/), or Professional, Enterprise, etc... _(**Minimum of Version 15.7** or higher, this is important, you WILL get issues with lesser versions)_
* .NET Framework 4.7.2 installed, get it here: https://www.microsoft.com/net/download/thank-you/net472?survey=false
* .NET Framework 4.7.2 developer pack, get it here: https://www.microsoft.com/net/download/thank-you/net472-developer-pack _(be sure this is the ENU file which will be named `NDP472-DevPack-ENU.exe`)_
* Clone the Umbraco repository using the `temp8` branch. If your git client doesn't support specifying the branch as you clone then use the command `git clone --single-branch -b temp8 <your fork url>`. _(If you clone the repo using the default v7 branch and then checkout the `temp8` branch you **might** get problems)_

### Start the solution

* Open the `/src/umbraco.sln` Visual Studio solution
* Start the solution (easiest way is to use `ctrl + F5`)
* When the solution is first built this may take some time since it will restore all nuget, npm and bower packages, build the .net solution and also build the angular solution
* When the website starts you'll see the Umbraco installer and just follow the prompts
* You're all set!

### Want to run from a zip instead?

If you just want to try out a few things, you can run the site from a zip file which you can download from here https://github.com/umbraco/Umbraco-CMS/releases/tag/temp8-cg18. 

We recommend running the site with the Visual Studio since you'll be able to remain up to date with the latest source code changes.

### Making code changes

* _[The process for making code changes in v8 is the same as v7](https://github.com/umbraco/Umbraco-CMS/blob/dev-v7/.github/CONTRIBUTING.md)_
* Any .NET changes you make you just need to compile
* Any Angular/JS changes you make you will need to make sure you are running the Gulp build. Easiest way to do this is from within Visual Studio in the `Task Runner Explorer`. You can find this window by pressing `ctrl + q` and typing in `Task Runner Explorer`. In this window you'll see all Gulp tasks, double click on the `dev` task, this will compile the angular solution and start a file watcher, then any html/js changes you make are automatically built.  
* When making js changes, you should have the chrome developer tools open to ensure that cache is disabled
* If you are only making C# changes and are not touching the UI code at all, you can significantly speed up the VS build by adding an empty file specifically called `~/src/preserve.belle`. The UI (Belle) build will then be skipped during a Rebuild.

### What to work on?

We are keeping track of [known issues and limitations here](http://issues.umbraco.org/issue/U4-11279). These line items will eventually be turned into actual tasks to be worked on. Feel free to help us keep this list updated if you find issues and even help fix some of these items. If there is a particular item you'd like to help fix please mention this on the task and we'll create a sub task for the item to continue discussion there.

There's [a list of tasks for v8 that haven't been completed](hhttps://github.com/umbraco/Umbraco-CMS/labels/release%2F8.0.0). If you are interested in helping out with any of these please mention this on the task. This list will be constantly updated as we begin to document and design some of the other tasks that still need to get done.

