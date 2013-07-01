var umbracoAppDev = angular.module('umbraco.httpbackend', ['umbraco', 'ngMockE2E']);

function urlRegex(url) {
  url = url.replace(/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g, "\\$&");
  return new RegExp("^" + url);
}

function getParameterByName(url, name) {
    name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
        results = regex.exec(url);
    return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}

var firsttry = true;
function returnUser(status, data, headers){
	var app = getParameterByName(data, "application");

	var mocked = {
					name: "Per Ploug",
					email: "test@test.com",
					emailHash: "f9879d71855b5ff21e4963273a886bfc",
					id: 0,
					locale: 'da-DK'
				};

	if(firsttry){
		firsttry = false;
		return [200, mocked, null];
	}else{
		return [200, mocked, null];	
	}
}

function returnApplicationTrees(status, data, headers){
	var app = getParameterByName(data, "application");
	var tree = _backendData.tree.getApplication(app);
	return [200, tree, null];
}

function returnAllowedChildren(status, data, headers){
	var types = [
          {name: "News Article", description: "Standard news article", alias: "newsArticle", id: 1234, cssClass:"file"},
          {name: "News Area", description: "Area to hold all news articles, there should be only one", alias: "newsArea", id: 1234, cssClass:"suitcase"},
          {name: "Employee", description: "Employee profile information page",  alias: "employee", id: 1234, cssClass:"user"}
          ];

	return [200, types, null];
}

function initBackEnd($httpBackend) {
	
	// returns the current list of phones
	$httpBackend
		.whenGET( urlRegex('/umbraco/Api/ContentType/GetAllowedChildren'))
		.respond( returnAllowedChildren);
		
	$httpBackend
			.whenPOST(urlRegex('/umbraco/UmbracoApi/Authentication/PostLogin'))
			.respond(returnUser);
		
	$httpBackend
		.whenGET('/umbraco/UmbracoApi/Authentication/GetCurrentUser')
		.respond(returnUser);

	$httpBackend
		.whenGET( urlRegex('/umbraco/UmbracoTrees/ApplicationTreeApi/GetApplicationTrees') )
		.respond(returnApplicationTrees);	

	// adds a new phone to the phones array
	$httpBackend.whenPOST('/phones').respond(function(method, url, data) {
	//phones.push(angular.fromJSON(data));
	});

	$httpBackend.whenGET(/^views\//).passThrough();
	$httpBackend.whenGET(/^js\//).passThrough();
	$httpBackend.whenGET(/^lib\//).passThrough();
	$httpBackend.whenGET(/^assets\//).passThrough();
}



umbracoAppDev.run(initBackEnd);
