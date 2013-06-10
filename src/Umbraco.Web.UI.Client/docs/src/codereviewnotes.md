#Codereview with Peter Bacon Darwin

##Office at cogworks:
71-75 Shelton Street 
London 
WC2H 9JQ

Meeting room 11 - 17

##Issues to go through:

###Structure, dependencies and external libraries
* review of modules structure and suggestions on how to handle loading things when needed.
* replace requireJs for dependency loading, so we dont have to load tinyMCE, googlemaps, etc
on app start $script, yepNope, labjs?

* get the app to load .aspx pages in an iframe instead of a "normal" view
	- write directive for loading templates to replace ng-include
	- if .aspx, load in iframe, 
	- if not found try default, finally load error msg	
* Javascript as resources from dlls? - add a scriptService to load these? - yes 
merge those resources into the umbraco.app.js file 



http://briantford.com/blog/huuuuuge-angular-apps.html


###Refactoring
* Convert tree into directive, recursive, lazy-load
	- $watchCollection $watch on the entire tree model
	- reuse the old tree plugin to inject into dom instead of into angular
	- 10 levels of digest limit
	- fine for CG, bad for release

* best practices for directives, what should we convert?
* other areas to convert?
	- for guidelines, look at angular/bootstrap-ui
	- replace our components with ng-bootstrap or angular-strap

###Application logic
* Authentication, force login, authenticate user against acccess to sections?
* whats the best way to handle urls, routes and state management, 
so the tree, sections etc, syncs with urls and the state of the application
* tinyMCE directive angular-ui 
* How to handle file-uploads
	- through a service?
	- ng-upload? or jquery-upload-plugin thingy?
* validation, ng-form $valid and directives should be enough
	- add remote directive: angular-app/admin/users/user-edit.js for directive code

###Dev experience
* H Way to handle templates with missing controller? -> replace ng-include? <- yup
	angular-app/samples/directives/fielddirective for code

	* H generel exception handling with feedback to log or notifications service
	* L jslint code on the server?  
		http://madskristensen.net/post/Verify-JavaScript-syntax-using-C.aspx
	- L automated setup of node, grunt, jasmine and karma, powershell and .sh? 	


###Testing	
* Best way to test against service data, simply mock data in memory, or better way?
* Testing dom manipulating components, like modals
* e2e testing
- teamcity intergration
- testing templates


#Notes
- Javascript as resources? - add a scriptService to load these?  nope, they will compile into umbraco.app.js
- capture errors with javascript code, to not load it into the combined files
(serverside jsLint) - mads blogpost for compiling js