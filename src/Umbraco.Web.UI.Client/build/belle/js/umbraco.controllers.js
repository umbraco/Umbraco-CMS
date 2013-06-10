'use strict';
/*! umbraco - v0.0.1-SNAPSHOT - 2013-06-10
 * http://umbraco.github.io/Belle
 * Copyright (c) 2013 Per Ploug, Anders Stenteberg & Shannon Deminick;
 * Licensed MIT
 */

define(['app', 'angular'], function (app, angular) {
//Handles the section area of the app
angular.module('umbraco').controller("NavigationController",
    function ($scope, navigationService) {
    
    //load navigation service handlers
    $scope.changeSection = navigationService.changeSection;    
    $scope.showTree = navigationService.showTree;
    $scope.hideTree = navigationService.hideTree;
    $scope.hideMenu = navigationService.hideMenu;
    $scope.showMenu = navigationService.showMenu;
    $scope.hideDialog = navigationService.hideDialog;
    $scope.hideNavigation = navigationService.hideNavigation;
    $scope.ui = navigationService.ui;    

    $scope.selectedId = navigationService.currentId;
    $scope.sections = navigationService.sections();
    
    //events
    $scope.$on("treeOptionsClick", function(ev, args){
            $scope.currentNode = args.node;
            args.scope = $scope;
            navigationService.showMenu(ev, args);
    });

    $scope.openDialog = function(currentNode,action,currentSection){
        navigationService.showDialog({
                                        scope: $scope,
                                        node: currentNode,
                                        action: action,
                                        section: currentSection});
    };
});


angular.module('umbraco').controller("SearchController", function ($scope, searchService, $log, navigationService) {

    var currentTerm = "";
    $scope.deActivateSearch = function(){
       currentTerm = ""; 
    };

    $scope.performSearch = function (term) {
        if(term != undefined && term != currentTerm){
            if(term.length > 3){
                $scope.ui.selectedSearchResult = -1;
                navigationService.showSearch();
                currentTerm = term;
                $scope.ui.searchResults = searchService.search(term, $scope.currentSection);
            }else{
                $scope.ui.searchResults = [];
            }
        }
    };    

    $scope.hideSearch = navigationService.hideSearch;

    $scope.iterateResults = function (direction) {
       if(direction == "up" && $scope.ui.selectedSearchResult < $scope.ui.searchResults.length) 
            $scope.ui.selectedSearchResult++;
        else if($scope.ui.selectedSearchResult > 0)
            $scope.ui.selectedSearchResult--;
    };

    $scope.selectResult = function () {
        navigationService.showMenu($scope.ui.searchResults[$scope.ui.selectedSearchResult], undefined);
    };
});


angular.module('umbraco').controller("DashboardController", function ($scope, $routeParams) {
    $scope.name = $routeParams.section;
});


//handles authentication and other application.wide services
angular.module('umbraco').controller("MainController", 
    function ($scope, $routeParams, $rootScope, notificationsService, userService, navigationService) {
    
    //also be authed for e2e test
    var d = new Date();
    var weekday = new Array("Super Sunday", "Manic Monday", "Tremendous Tuesday", "Wonderfull Wednesday", "Thunder Thursday", "Friendly Friday", "Shiny Saturday");
    $scope.today = weekday[d.getDay()];
    

    $scope.signin = function () {
        $scope.authenticated = userService.authenticate($scope.login, $scope.password);

        if($scope.authenticated){
            $scope.user = userService.getCurrentUser();
        }
    };

    $scope.signout = function () {
        userService.signout();
        $scope.authenticated = false;
    };
    

    //subscribes to notifications in the notification service
    $scope.notifications = notificationsService.current;
    $scope.$watch('notificationsService.current', function (newVal, oldVal, scope) {
        if (newVal) {
            $scope.notifications = newVal;
        }
    });

    $scope.removeNotification = function(index) {
        notificationsService.remove(index);
    };

    $scope.closeDialogs = function(event){

        $rootScope.$emit("closeDialogs");

        if(navigationService.ui.stickyNavigation && $(event.target).parents(".umb-modalcolumn").size() == 0){ 
            navigationService.hideNavigation();
        }
    };

    if (userService.authenticated) {
        $scope.signin();
    }
});

//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.ContentPickerController", 
	function ($scope) {	
	
	$scope.$on("treeNodeSelect", function(event, args){
		args.event.preventDefault();	
		$(args.event.target.parentElement).find("i.umb-tree-icon").attr("class", "icon umb-tree-icon sprTree icon-check blue");
		$scope.select(args.node);
	});
});
//used for the macro picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.MacroPickerController", function ($scope, macroFactory) {	
	$scope.macros = macroFactory.all(true);
	$scope.dialogMode = "list";

	$scope.configureMacro = function(macro){
		$scope.dialogMode = "configure";
		$scope.dialogData.macro = macroFactory.getMacro(macro.alias);
	};
});
//used for the media picker dialog
angular.module("umbraco")
.controller("Umbraco.Dialogs.MediaPickerController",
	function ($scope, mediaResource) {	
	$scope.images = mediaResource.rootMedia();
});
angular.module("umbraco").controller("Umbraco.Common.LegacyController", 
	function($scope, $routeParams){
		$scope.legacyPath = decodeURI($routeParams.p);
	});
angular.module('umbraco')
.controller("Umbraco.Editors.ContentCreateController",
	function ($scope, $routeParams,contentTypeResource) {
	$scope.allowedTypes  = contentTypeResource.getAllowedTypes($scope.currentNode.id);
});
angular.module("umbraco")
.controller("Umbraco.Editors.ContentEditController",
	function ($scope, $routeParams, contentResource, notificationsService) {
	
	if($routeParams.create)
		$scope.content = contentResource.getContentScaffold($routeParams.id, $routeParams.doctype);
	else
		$scope.content = contentResource.getContent($routeParams.id);


	$scope.saveAndPublish = function (cnt) {
		cnt.publishDate = new Date();
		contentResource.publishContent(cnt);
		notificationsService.success("Published", "Content has been saved and published");
	};

	$scope.save = function (cnt) {
		cnt.updateDate = new Date();
		contentResource.saveContent(cnt);
		notificationsService.success("Saved", "Content has been saved");
	};
});
angular.module("umbraco").controller("Umbraco.Editors.CodeMirrorController", function ($scope, $rootScope) {
    require(
        [
            'css!../lib/codemirror/js/lib/codemirror.css',
            'css!../lib/codemirror/css/umbracoCustom.css',
            'codemirrorHtml'
        ],
        function () {

            var editor = CodeMirror.fromTextArea(
                                    document.getElementById($scope.model.alias), 
                                    {
                                        mode: CodeMirror.modes.htmlmixed, 
                                        tabMode: "indent"
                                    });

            editor.on("change", function(cm) {
                $rootScope.$apply(function(){
                    $scope.model.value = cm.getValue();   
                });
            });

        });
});
//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')
.controller("Umbraco.Editors.ContentPickerController",
	function($scope, dialogService){
    $scope.openContentPicker =function(value){
            var d = dialogService.contentPicker({scope: $scope, callback: populate});
    };

    function populate(data){
        $scope.model.value = data.selection;
    }
});
angular.module("umbraco").controller("Umbraco.Editors.DatepickerController", 
    function ($scope, notificationsService) {
    require(
        [
            'views/propertyeditors/umbraco/datepicker/bootstrap.datepicker.js',
            'css!/belle/views/propertyeditors/umbraco/datepicker/bootstrap.datepicker.css'
        ],
        function () {
            //The Datepicker js and css files are available and all components are ready to use.

            // Get the id of the datepicker button that was clicked
            var pickerId = $scope.model.alias;

            // Open the datepicker and add a changeDate eventlistener
            $("#" + pickerId).datepicker({
                format: "dd/mm/yyyy",
                autoclose: true
            }).on("changeDate", function (e) {
                // When a date is clicked the date is stored in model.value as a ISO 8601 date
                $scope.model.value = e.date.toISOString();
            });
        }
    );
});

angular.module("umbraco")
.controller("Umbraco.Editors.GoogleMapsController", 
    function ($rootScope, $scope, notificationsService, $timeout) {
    require(
        [
            'async!http://maps.google.com/maps/api/js?sensor=false'
        ],
        function () {
            //Google maps is available and all components are ready to use.
            var valueArray = $scope.model.value.split(',');
            var latLng = new google.maps.LatLng(valueArray[0], valueArray[1]);
            
            var mapDiv = document.getElementById($scope.model.alias + '_map');
            var mapOptions = {
                zoom: $scope.model.config.zoom,
                center: latLng,
                mapTypeId: google.maps.MapTypeId[$scope.model.config.mapType]
            };

            var map = new google.maps.Map(mapDiv, mapOptions);
            var marker = new google.maps.Marker({
                map: map,
                position: latLng,
                draggable: true
            });
             
            google.maps.event.addListener(marker, "dragend", function(e){
                var newLat = marker.getPosition().lat();
                var newLng = marker.getPosition().lng();
            
                //here we will set the value
                $scope.model.value = newLat + "," + newLng;

                //call the notication engine
                $rootScope.$apply(function () {
                    notificationsService.warning("Your dragged a marker to", $scope.model.value);
                });
            });

            //hack to hook into tab switching for map resizing
            $('a[data-toggle="tab"]').on('shown', function (e) {
                google.maps.event.trigger(map, 'resize');
            });

        }
    );    
});
'use strict';
//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module("umbraco").controller("Umbraco.Editors.GridController", 
  function($rootScope, $scope, dialogService, $log){
    //we most likely will need some iframe-motherpage interop here
    
    //we most likely will need some iframe-motherpage interop here
       $scope.openMediaPicker =function(){
               var d = dialogService.mediaPicker({scope: $scope, callback: renderImages});
       };

       $scope.openPropertyDialog =function(){
               var d = dialogService.property({scope: $scope, callback: renderProperty});
       };

       $scope.openMacroDialog =function(){
               var d = dialogService.macroPicker({scope: $scope, callback: renderMacro});
       };

       function renderProperty(data){
          $scope.currentElement.html("<h1>boom, property!</h1>"); 
       }

       function renderMacro(data){
       //   $scope.currentElement.html( macroFactory.renderMacro(data.macro, -1) ); 
       }
      
       function renderImages(data){
           var list = $("<ul class='thumbnails'></ul>")
           $.each(data.selection, function(i, image) {
               list.append( $("<li class='span2'><img class='thumbnail' src='" + image.src + "'></li>") );
           });

           $scope.currentElement.html( list[0].outerHTML); 
       }

       $(window).bind("umbraco.grid.click", function(event){

           $scope.$apply(function () {
               $scope.currentEditor = event.editor;
               $scope.currentElement = $(event.element);

               if(event.editor == "macro")
                   $scope.openMacroDialog();
               else if(event.editor == "image")
                   $scope.openMediaPicker();
               else
                   $scope.propertyDialog();
           });
       })
});
angular.module("umbraco")
    .controller("Umbraco.Editors.ListViewController", 
        function ($rootScope, $scope, contentResource, contentTypeResource) {
        $scope.options = {
            take: 10,
            offset: 0,
            filter: '',
            sortby: 'id',
            order: "desc"
        };

        $scope.pagination = new Array(100);
        $scope.listViewAllowedTypes = contentTypeResource.getAllowedTypes($scope.content.id);
        
        $scope.next = function(){
            if($scope.options.offset < $scope.listViewResultSet.pages){
                $scope.options.offset++;
                $scope.reloadView();    
            }
        };

        $scope.goToOffset = function(offset){
            $scope.options.offset = offset;
            $scope.reloadView();
        };

        $scope.sort = function(field){
            $scope.options.sortby = field;
            
            if(field !== $scope.options.sortby){
                if($scope.options.order === "desc"){
                    $scope.options.order = "asc";
                }else{
                    $scope.options.order = "desc";    
                }
            }
            $scope.reloadView();
        };

        $scope.prev = function(){
            if($scope.options.offset > 0){
                $scope.options.offset--;    
                
                $scope.reloadView();
            }
        };

        /*Loads the search results, based on parameters set in prev,next,sort and so on*/
        /*Pagination is done by an array of objects, due angularJS's funky way of monitoring state
        with simple values */
        $scope.reloadView = function(){
                $scope.listViewResultSet = contentResource.getChildren($scope.content.id, $scope.options);
                
                $scope.pagination = [];
                for (var i = $scope.listViewResultSet.pages - 1; i >= 0; i--) {
                        $scope.pagination[i] = {index: i, name: i+1};
                };
                
                if($scope.options.offset > $scope.listViewResultSet.pages){
                    $scope.options.offset = $scope.listViewResultSet.pages;
                }        
        };

        $scope.reloadView();
});
//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco').controller("Umbraco.Editors.MediaPickerController", 
	function($rootScope, $scope, dialogService, $log){
    $scope.openMediaPicker =function(value){
            var d = dialogService.mediaPicker({scope: $scope, callback: populate});
    };

    function populate(data){
    	$log.log(data.selection);
        $scope.model.value = data.selection;    
    }
});
angular.module("umbraco")
    .controller("Umbraco.Editors.RTEController", 
    function($rootScope, $scope, dialogService, $log){
    require(
        [
            'tinymce'
        ],
        function (tinymce) {

            tinymce.DOM.events.domLoaded = true;
            tinymce.init({
                selector: "#" + $scope.model.alias + "_rte",
                skin: "umbraco",
                menubar : false,
                statusbar: false,
                height: 340,
                toolbar: "bold italic | styleselect | alignleft aligncenter alignright | bullist numlist | outdent indent | link image mediapicker",
                setup : function(editor) {
                        
                        editor.on('blur', function(e) {
                            $scope.$apply(function(){
                                //$scope.model.value = e.getBody().innerHTML;
                                $scope.model.value = editor.getContent();
                            })
                        });

                        editor.addButton('mediapicker', {
                            icon: 'media',
                            tooltip: 'Media Picker',
                            onclick: function(){
                                dialogService.mediaPicker({scope: $scope, callback: function(data){
                                 
                                    //really simple example on how to intergrate a service with tinyMCE
                                    $(data.selection).each(function(i,img){
                                            var data = {
                                                src: img.thumbnail,
                                                style: 'width: 100px; height: 100px',
                                                id : '__mcenew'
                                            };
                                            
                                            editor.insertContent(editor.dom.createHTML('img', data));
                                            var imgElm = editor.dom.get('__mcenew');
                                            editor.dom.setAttrib(imgElm, 'id', null);
                                    });    
                                       

                                }});
                            }
                        });

            
                  }
            });


            $scope.openMediaPicker =function(value){
                    var d = dialog.mediaPicker({scope: $scope, callback: populate});
            };

            function bindValue(inst){
                $log.log("woot");

                $scope.$apply(function(){
                    $scope.model.value = inst.getBody().innerHTML;
                })
            }

            function myHandleEvent(e){
                $log.log(e);
            }

            function populate(data){
                $scope.model.value = data.selection;    
            }

        });
});
angular.module("umbraco")
.controller("Umbraco.Editors.TagsController",
	function($rootScope, $scope, $log, tagsResource) {	
		require(
			[
			'/belle/views/propertyeditors/umbraco/tags/bootstrap-tags.custom.js',
			'css!/belle/views/propertyeditors/umbraco/tags/bootstrap-tags.custom.css'
			],function(){

			// Get data from tagsFactory
			$scope.tags = tagsResource.getTags("group");

			// Initialize bootstrap-tags.js script
			var tags = $('#' + $scope.model.alias + "_tags").tags({
				tagClass: 'label-inverse'
			});

			$.each($scope.tags, function(index, tag) {
				tags.addTag(tag.label);
			});
		});
	}
);
//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco').controller("Umbraco.Editors.EmbeddedContentController", 
	function($rootScope, $scope, $log){
    
	$scope.showForm = false;
	$scope.fakeData = [];

	$scope.create = function(){
		$scope.showForm = true;
		$scope.fakeData = angular.copy($scope.model.config.fields);
	};

	$scope.show = function(){
		$scope.showCode = true;
	};

	$scope.add = function(){
		$scope.showForm = false;
		if ( !($scope.model.value instanceof Array)) {
			$scope.model.value = [];
		}

		$scope.model.value.push(angular.copy($scope.fakeData));
		$scope.fakeData = [];
	};
});

return angular;
});