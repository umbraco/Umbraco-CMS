var umbracoAppDev = angular.module('umbraco.httpbackend', ['umbraco', 'ngMockE2E', 'umbraco.mocks']);


function initBackEnd($httpBackend, contentMocks, treeMocks, userMocks, contentTypeMocks, sectionMocks) {

	console.log("httpBackend inited");
	
    //Register mocked http responses
    contentMocks.register();
    sectionMocks.register();
    treeMocks.register();

    userMocks.register();

    contentTypeMocks.register();
    
	$httpBackend.whenGET(/^views\//).passThrough();
	$httpBackend.whenGET(/^js\//).passThrough();
	$httpBackend.whenGET(/^lib\//).passThrough();
	$httpBackend.whenGET(/^assets\//).passThrough();
}



umbracoAppDev.run(initBackEnd);
