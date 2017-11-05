# First Timers

We hear sometimes that people find it difficult to wrap their head around the contribution guidelines we have. This page shows you how to get started in small steps. The lengthy documentation we'll save for you to read another day. Get started, do something, instead of being paralyzed in fear of doing it wrong. Don't worry, we're a friendly bunch and love to help.

## Pull Request Process

1. **Fork** the repository
2. **Clone** the fork to your local machine
3. **Build** & **Run** the solution
4. Create new **branch** with a proper name and then make changes
5. **Commit** & **Push** to your fork
6. Submit a **Pull Request**

## Fork the repository

The first thing to do is go to https://github.com/umbraco/Umbraco-CMS and click the "Fork" button at the top right. This copies the Umbraco repository into your own account so you can start editing in a safe environment.

![Forking the repo](https://our.umbraco.org/media/7514491/2016-06-21_132516.png?width=958&height=524)

After clicking the "Fork" button you'll have to wait a few seconds and the repository will land on your GitHub account.

## Clone the fork to your local machine

You can follow the instructions on GitHub to get the repository cloned to your local machine. If you have GitHub for Windows installed this should be easy with the click of "Open in Desktop".

![Cloning the repo](https://our.umbraco.org/media/7514492/2016-06-21_133506.png?width=456&height=196)

If you have any other Git client installed, copy the URL and past it into the clone dialog of your preferred Git client. 

If you want to see the process done with GitHub client for Windows, watch 
[this video](http://www.youtube.com/watch?v=BhzOoyvCDcU).

## Build & run the solution

Once the repository is cloned on your local machine, open the folder where you cloned it to and go into the "build" folder. From there you can run `build.bat` file. 

This will set up everything for you and runs MSBuild and the Gulp build for you. Don't worry if you have no idea what that all means, it's all automatic and should look something like on the video [here](https://www.youtube.com/watch?v=nZHQeB3mCzo).

Now that everything is set up for you, go into the "src" folder and open `umbraco.sln` in Visual Studio. 

Once that loads set the startup project to **Umbraco.Web.UI** and hit **F5** or press the **"Play" button** to start the solution. 

After a few minutes, you will see the Umbraco installer which will guide you through setting up a new Umbraco website. The process is exactly the same as for the standard Umbraco installation. You can preview it on the video [here](https://www.youtube.com/watch?v=7CMdRf-fxlg).

Once this site is running you can start changing the code in Umbraco anywhere you need to change it. When you're done changing, hit **F5 / the "Play" button**, verify that your fix worked in the Umbraco site that you now have and then it's time to commit your changes.

## Create new branch and make changes

First of all, make sure to create a new branch from the proper development branch (e.g. `dev-v7` for Umbraco 7 development). This ensures that you can work on multiple pull requests at once. Otherwise you'd have to wait for us to evaluate the PR first and merge it in before you can do anything else. 

It's a great idea to name the branch after the issue your fixing on the issue tracker. If you're working on issue U4-7879 then create a branch named U4-7879.

![Create new branch](https://our.umbraco.org/media/7514493/2016-06-21_151310.png?width=579&height=228)

Then you can commit your changes. Make sure to commit to the correct branch.

![Commit your changes](https://our.umbraco.org/media/7514494/2016-06-21_151415.png?width=1000&height=622.5328947368421)

Finally, you can push your changes. In the GitHub for Windows app this is called "Sync" (top right) in other git applications it's usually called "Push". This pushes the changes to your personal copy of the Umbraco repository after which you're ready to send a pull request.

## Submit a Pull Request

If you go to your fork on GitHub you can switch to the branch you've just created. From that branch you can press the "Compare & pull request" button.

![Submit a Pull Request](https://our.umbraco.org/media/7514496/2016-06-21_152345.png?width=996&height=221)

You'll be asked to give a description after which you can open the pull request. You can follow our [Pull Request Template](PULL_REQUEST_TEMPLATE.md) to make it clear and descriptive. **Remember this is the start of a conversation, you can open a pull request with some example code and ask us how to continue.** If you already have working code that's great too, we can and will give you feedback on the pull request.

You will then be redirected to an overview screen where you will also see the build status. Each pull request will be automatically built for you on our build server so that we know it builds and the unit tests still work.

![PR comments](https://our.umbraco.org/media/7514498/2016-06-21_152737.png?width=784&height=565)

## That's it!

Again, we might ask questions or ask you to correct something. This is easy: make the corrections in this branch an push them to your fork again. The pull request automatically updates with the additional commit so we can review it again. If all is well, we'll merge the code and your commits are forever part of Umbraco!