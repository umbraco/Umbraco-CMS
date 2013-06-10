#Things to do 

##Structure
- One module pr file idea, instead of registering everything on app.js
- Have core services, resources and other common items under umbraco

- third party modules outside of the root umbraco module, but registered in app.js
- to access 3rd party service:
	ecomEditor.controller.js
	angular.module("umbraco.myeditor", ["ecommerce.services"]).controller("ecom.editor", 
		function("inventoryFactory"){
			do things...
		});

- best way to setup services and controllers are: 
.controller("name",[
	"service",
	"dependency",
	function(s, d){

	}
]);

- move logic from controllers to services, especcially around navigation
	- easier for testing
	- only keep view interactions, everything into a service
	- looser testing on controllers
	- for testing the dialogs, look in angular source or angular bootstrap projects

##Routing
Change /section/page/id to /section/area/page/id to support all section scenarios
Have a fallback to defaults?

##Legacy
- for UmbClientTools we can access the services in angular from 
	angular.element("body").injector().get("notifications");
- rootscope available in same location
- the bootstrap method returns the injector


##ScriptLoaderService
	- Service to load required scripts for a controller using $script
	- remove requirejs dependency as it makes things muddy

##Authentication
Angular-app: common/security/interceptor.js , intercept http requests

##Promises
	Use promises pattern for all our services
	$http.get(url)
		.then(function(response){
			return response.data;
		}, function(response){
			return $q.reject("http failed");
		}).then(function(data){
			alert("our data:" + data);
		})

##Think about rest services and authentication
Usecase: member picker editor, which fetches member-data

##Avoid $resource and instead use $http


Sublime linter