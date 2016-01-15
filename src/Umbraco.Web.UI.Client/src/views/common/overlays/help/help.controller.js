angular.module("umbraco")
    .controller("Umbraco.Overlays.HelpController", function ($scope, $location, $routeParams, helpService, userService, localizationService) {
        $scope.section = $routeParams.section;
        $scope.version = Umbraco.Sys.ServerVariables.application.version + " assembly: " + Umbraco.Sys.ServerVariables.application.assemblyVersion;
        $scope.model.subtitle = "Umbraco version" + " " + $scope.version;

        if(!$scope.model.title) {
            $scope.model.title = localizationService.localize("general_help");
        }

        if(!$scope.section){
            $scope.section = "content";
        }

        $scope.sectionName = $scope.section;

        var rq = {};
        rq.section = $scope.section;

        //translate section name
        localizationService.localize("sections_" + rq.section).then(function (value) {
            $scope.sectionName = value;
        });

        userService.getCurrentUser().then(function(user){

        	rq.usertype = user.userType;
        	rq.lang = user.locale;

    	    if($routeParams.url){
    	    	rq.path = decodeURIComponent($routeParams.url);

    	    	if(rq.path.indexOf(Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath) === 0){
    				rq.path = rq.path.substring(Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath.length);
    			}

    			if(rq.path.indexOf(".aspx") > 0){
    				rq.path = rq.path.substring(0, rq.path.indexOf(".aspx"));
    			}

    	    }else{
    	    	rq.path = rq.section + "/" + $routeParams.tree + "/" + $routeParams.method;
    	    }

    	    helpService.findHelp(rq).then(function(topics){
    	    	$scope.topics = topics;
    	    });

    	    helpService.findVideos(rq).then(function(videos){
    	        $scope.videos = videos;
    	    });

        });


    });
