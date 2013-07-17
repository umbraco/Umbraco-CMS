#Getting up and running with Belle

_The super fast introduction to getting belle running on your local machine, both as a pre-built environment, and with the full setup with unit-tests, grunt-tasks and node._

##Running the prebuilt site

###Windows
Right-click the `/build` folder and choose "open in webmatrix", run the website in webmatrix and browse to `localhost:9999/Belle/`, this should display the Belle login screen

*Port 9999 should be used because that is the target site that the grunt build command mentioned below will launch*

###OSX
Open a terminal inside the "/build" folder and run the command:

	python -m SimpleHTTPServer 9999

This will start a local webserver, hosting the site on `localhost:9999` browse to localhost:9999/Belle/ which should display the belle login screen.

##Uing the dev environment
_The dev environment is tad more tricky to get running, since it depends on a number of unit tests and automated tools, to produce the contents of the /build folder_

_The dev environment is cross platform, so will work on both osx and windows, and do not currently have any dependencies to .net_

###Install node.js
We need node to run tests and automated less compiling and other automated tasks. go to http://nodejs.org. Node.js is a powerfull javascript engine, which allows us to run all our tests and tasks written in javascript locally.

*note:* On windows you might need to restart explorer.exe to register node.


###Install dependencies
Next we need to install all the required packages. This is done with the package tool, included with node.js, open /Umbraco.Belle.Client in cmd.exe or osx terminal and run the command:

	npm install

this will fetch all needed packages to your local machine.


###Install grunt globally
Grunt is a task runner for node.js, and we use it for all automated tasks in the build process. For convenience we need to install it globally on your machine, so it can be used directly in cmd.exe or the terminal.

So run the command:

	npm install grunt-cli -g

*note:* On windows you might need to restart explorer.exe to register the grunt cmd.

*note:* On OSX you might need to run:
	
	sudo npm install grunt-cli -g

Now that you have node and grunt installed, you can open `/Umbraco.Belle.Client` in either `cmd.exe` or terminal and run: 

	grunt dev

This will build the site, merge less files, run tests and create the /Build folder, launch the web browser and monitor changes.

###Automated builds and tests

grunt dev will continue to run in the background monitoring changes to files. When changes are detected it will rebuild the JS and also run the unit tests.






