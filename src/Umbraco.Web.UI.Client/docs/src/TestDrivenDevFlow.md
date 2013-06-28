#Test-driven developement flow for Umbraco 7

_This document tries to outline what is required to have a test-driven setup for
angular developement in Umbraco 7. It goes through the setup process as well as how
to add new services that requires mocking as well as how to use grunt to run tests automaticly._

##Setup
Make sure to have all the node dependencies in order when you start, these are updated regularly in case we need to go to a new version of a dependency, or new dependencies are added.

Simply run open a terminal / cmd in the Umbraco.Web.Ui.Client folder and run:

	npm install

This should setup the entire grunt,karma and jsint setup we use for tests and pruning.

##Automated testing
To start working on the client files, and have them automaticly built and merged into the client project, as well as the VS project, simply run the command

	grunt dev

This will start a webserver on :8080 and tell karma to run tests every time a .js or .less file is changed. 

After linting and tests have passed, all the client files are copied to umrbaco.web.ui/umbraco folder, so it also keeps the server project uptodate on any client changes. This should all happen in the background.

##Adding a new service
The process for adding or modifying a service should always be based on passed tests. So if we need to change the footprint of the contentservice, and the way any controller calls this service, we need to make sure the tests passes with our mocked services.

This ensures 3 things: 
- we test our controllers
- we test our services
- we always have mocked data available, if you want to run the client without IIS

###Example: 
We add a service for fetching macros from the database, the initial implementation should happen of this service should happen in `/src/common/resources/macro.resource.js`

The macro.resource.js calls `$http` as normal, but no server implementation should be needed at this point. 

Next, we describe how the rest service should return data, this is done in /common/mocks/umbraco.httpbackend.js, where we can define what data a certain url
would return. 

--- in progress ---





