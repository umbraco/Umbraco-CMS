#Belle Lab Tasks

##Applcation Structure
- Work on dialogs, plenty to choose from
- A reuseable tree component for pickers
- migrate all the navigation stuff into a navigation service
- a scriptloading service to replace requireJs, with labJs, $script or similiar
- reusable modal component (show in left side, right side, generic closing events etc)


##Components
- tabs directive
- date picker
- tabs property editor
- localization strategy?
	- localize filter: {{This is my default english value,content.area.key | localize}}
	- So, it will use the default value if there is none found for the key, and register this
	  value in the localize service, keys should not contain commas
 	

##Chores
- Write a test
- Write docs
- Automate something
	- OSX:
		- start webserver
		- start grunt watch
		- improve test output?
		- phantomJs?
	- windows
		- start webserver
		- start grunt
		- install node stuff?
		- register chrome_bin in path
		- or register phantomJS?
