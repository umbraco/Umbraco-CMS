'use strict';
/*! umbraco - v0.0.1-SNAPSHOT - 2013-06-03
 * http://umbraco.github.io/Belle
 * Copyright (c) 2013 Per Ploug, Anders Stenteberg & Shannon Deminick;
 * Licensed MIT
 */

define(['app', 'angular'], function (app, angular) {
//Handles the section area of the app
angular.module('umbraco').controller("NavigationController", 
    function ($scope, $window, $log, tree, section, $rootScope, $routeParams, dialog) {
    
    $scope.currentSection = $routeParams.section;
    $scope.selectedId = $routeParams.id;
    $scope.sections = section.all();

    $scope.ui.mode = setMode;
    $scope.ui.mode("default-onload");

    $scope.$on("treeOptionsClick", function(ev, node){
            $scope.showMenu(node, ev);
    });

    $scope.openSection = function (selectedSection) {
        //reset everything
        if($scope.ui.stickyNavigation){
            $scope.ui.mode("default-opensection");
            section.setCurrent(selectedSection.alias);
            $scope.currentSection = selectedSection.alias;
            $scope.showSectionTree(selectedSection);
        }
    };

    $scope.showSectionTree = function (section) {
        if(!$scope.ui.stickyNavigation){
            $("#search-form input").focus();
            $scope.currentSection = section.alias;
            $scope.ui.mode("tree");
        }
    };

    $scope.hideSectionTree = function () {
        if(!$scope.ui.stickyNavigation){
            $scope.ui.mode("default-hidesectiontree");
        }
    };

    $scope.showMenu = function (node, event) {
        $log.log("testing the show meny");

        if(event != undefined && node.defaultAction && !event.altKey){
            //hack for now, it needs the complete action object to, so either include in tree item json
            //or lookup in service...
            var act = {
                        alias: node.defaultAction,
                        name: node.defaultAction
                    };
             $scope.showContextDialog(node, act);
       }else{
            $scope.contextMenu = tree.getActions({node: node, section: $scope.section});
            $scope.currentNode = node;
            $scope.menuTitle = node.name;
            $scope.selectedId = node.id;
            $scope.ui.mode("menu");
        }
    };

    $scope.hideContextMenu = function () {
        $scope.selectedId = $routeParams.id;
        $scope.contextMenu = [];
        $scope.ui.mode("tree");
    };

    $scope.showContextDialog = function (item, action) {
        $scope.ui.mode("dialog");

        $scope.currentNode = item;
        $scope.dialogTitle = action.name;

        var templateUrl = "views/" + $scope.currentSection + "/" + action.alias + ".html";
        var d = dialog.append({container: $("#dialog div.umb-panel-body"), scope: $scope, template: templateUrl });
    };    

    $scope.hideContextDialog = function () {
        $scope.showContextMenu($scope.currentNode, undefined);
    };    

    $scope.hideNavigation = function () {
        $scope.ui.mode("default-hidenav");
    };

    function loadTree(section) {
        $scope.currentSection = section;
        
    }

    //function to turn navigation areas on/off
    function setMode(mode){

            switch(mode)
            {
            case 'tree':
                $scope.ui.showNavigation = true;
                $scope.ui.showContextMenu = false;
                $scope.ui.showContextMenuDialog = false;
                $scope.ui.stickyNavigation = false;
                break;
            case 'menu':
                $scope.ui.showNavigation = true;
                $scope.ui.showContextMenu = true;
                $scope.ui.showContextMenuDialog = false;
                $scope.ui.stickyNavigation = true;
                break;
            case 'dialog':
                $scope.ui.stickyNavigation = true;
                $scope.ui.showNavigation = true;
                $scope.ui.showContextMenu = false;
                $scope.ui.showContextMenuDialog = true;
                break;
            case 'search':
                $scope.ui.stickyNavigation = false;
                $scope.ui.showNavigation = true;
                $scope.ui.showContextMenu = false;
                $scope.ui.showSearchResults = true;
                $scope.ui.showContextMenuDialog = false;
                break;      
            default:
                $scope.ui.showNavigation = false;
                $scope.ui.showContextMenu = false;
                $scope.ui.showContextMenuDialog = false;
                $scope.ui.showSearchResults = false;
                $scope.ui.stickyNavigation = false;
                break;
            }
    }
});


angular.module('umbraco').controller("SearchController", function ($scope, search, $log) {

    var currentTerm = "";
    $scope.deActivateSearch = function(){
       currentTerm = ""; 
    };

    $scope.performSearch = function (term) {
        if(term != undefined && term != currentTerm){
            if(term.length > 3){
                $scope.ui.selectedSearchResult = -1;
                $scope.ui.mode("search");

                currentTerm = term;
                $scope.ui.searchResults = search.search(term, $scope.currentSection);

            }else{
                $scope.ui.searchResults = [];
            }
        }
    };    

    $scope.hideSearch = function () {
       $scope.ui.mode("default-hidesearch");
    };

    $scope.iterateResults = function (direction) {
       if(direction == "up" && $scope.ui.selectedSearchResult < $scope.ui.searchResults.length) 
            $scope.ui.selectedSearchResult++;
        else if($scope.ui.selectedSearchResult > 0)
            $scope.ui.selectedSearchResult--;
    };

    $scope.selectResult = function () {
        $scope.showContextMenu($scope.ui.searchResults[$scope.ui.selectedSearchResult], undefined);
    };
});


angular.module('umbraco').controller("DashboardController", function ($scope, $routeParams) {
    $scope.name = $routeParams.section;
});


//handles authentication and other application.wide services
angular.module('umbraco').controller("MainController", 
    function ($scope, notifications, $routeParams, userFactory, localizationFactory) {
    
    //also be authed for e2e test
    var d = new Date();
    var weekday = new Array("Super Sunday", "Manic Monday", "Tremendous Tuesday", "Wonderfull Wednesday", "Thunder Thursday", "Friendly Friday", "Shiny Saturday");
    $scope.today = weekday[d.getDay()];

    $scope.ui = {
        showTree: false,
        showSearchResults: false,
        mode: undefined
    };


    $scope.signin = function () {
        $scope.authenticated = userFactory.authenticate($scope.login, $scope.password);

        if($scope.authenticated){
            $scope.user = userFactory.getCurrentUser();
            $scope.localization = localizationFactory.getLabels($scope.user.locale);
        }
    };

    $scope.signout = function () {
        userFactory.signout();
        $scope.authenticated = false;
    };
    

    //subscribes to notifications in the notification service
    $scope.notifications = notifications.current;
    $scope.$watch('notifications.current', function (newVal, oldVal, scope) {
        if (newVal) {
            $scope.notifications = newVal;
        }
    });

    //subscribes to auth status in $user
    $scope.authenticated = userFactory.authenticated;
    $scope.$watch('userFactory.authenticated', function (newVal, oldVal, scope) {
        if (newVal) {
            $scope.authenticated = newVal;
        }
    });

    $scope.removeNotification = function(index) {
        notifications.remove(index);
    };

    $scope.closeDialogs = function(event){
        if($scope.ui.stickyNavigation && $(event.target).parents(".umb-modalcolumn").size() == 0){ 
            $scope.ui.mode("default-closedialogs");
        }
    };

    if (userFactory.authenticated) {
        $scope.signin();
    }
});

//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.ContentPickerController", function ($scope, mediaFactory) {	
	
	$scope.$on("treeNodeSelect", function(event, args){
		$(args.event.target.parentElement).find("i").attr("class", "icon umb-tree-icon sprTree icon-check blue");
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
angular.module("umbraco").controller("Umbraco.Dialogs.MediaPickerController", function ($scope, mediaFactory) {	
	$scope.images = mediaFactory.rootMedia();
});
angular.module("umbraco").controller("Umbraco.Common.LegacyController", 
	function($scope, $routeParams){
		$scope.legacyPath = decodeURI($routeParams.p);
	});
angular.module('umbraco').controller("Umbraco.Editors.ContentCreateController", function ($scope, $routeParams,contentTypeFactory) {	
	$scope.allowedTypes  = contentTypeFactory.getAllowedTypes($scope.currentNode.id);	
});
angular.module("umbraco").controller("Umbraco.Editors.ContentEditController", function ($scope, $routeParams, contentFactory, notifications) {
	
	if($routeParams.create)
		$scope.content = contentFactory.getContentScaffold($routeParams.parentId, $routeParams.doctype);
	else
		$scope.content = contentFactory.getContent($routeParams.id);


	$scope.saveAndPublish = function (cnt) {
		cnt.publishDate = new Date();
		contentFactory.publishContent(cnt);

		notifications.success("Published", "Content has been saved and published");
	};

	$scope.save = function (cnt) {
		cnt.updateDate = new Date();

		contentFactory.saveContent(cnt);
		notifications.success("Saved", "Content has been saved");
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
angular.module('umbraco').controller("Umbraco.Editors.ContentPickerController", function($rootScope, $scope, dialog, $log){
    $scope.openContentPicker =function(value){
            var d = dialog.contentPicker({scope: $scope, callback: populate});
    };

    function populate(data){
        $scope.model.value = data.selection;    
    }
});
angular.module("umbraco").controller("Umbraco.Editors.DatepickerController", function ($rootScope, $scope, notifications, $timeout) {
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

angular.module("umbraco").controller("Umbraco.Editors.GoogleMapsController", function ($rootScope, $scope, notifications, $timeout) {
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
                    notifications.warning("Your dragged a marker to", $scope.model.value);
                });
            });

            //hack to hook into tab switching for map resizing
            $('a[data-toggle="tab"]').on('shown', function (e) {
                google.maps.event.trigger(map, 'resize');
            })


        }
    );    
});
'use strict';
//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module("umbraco").controller("Umbraco.Editors.GridController", function($rootScope, $scope, dialog, $log, macroFactory){
    //we most likely will need some iframe-motherpage interop here
    
    //we most likely will need some iframe-motherpage interop here
       $scope.openMediaPicker =function(){
               var d = dialog.mediaPicker({scope: $scope, callback: renderImages});
       };

       $scope.openPropertyDialog =function(){
               var d = dialog.property({scope: $scope, callback: renderProperty});
       };

       $scope.openMacroDialog =function(){
               var d = dialog.macroPicker({scope: $scope, callback: renderMacro});
       };

       function renderProperty(data){
          $scope.currentElement.html("<h1>boom, property!</h1>"); 
       }

       function renderMacro(data){
          $scope.currentElement.html( macroFactory.renderMacro(data.macro, -1) ); 
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
    .controller("Umbraco.Editors.ListViewController", function ($rootScope, $scope, contentFactory, contentTypeFactory) {
        $scope.options = {
            take: 10,
            offset: 0,
            filter: '',
            sortby: 'id',
            order: "desc"
        };

        $scope.pagination = new Array(100);
        $scope.listViewAllowedTypes = contentTypeFactory.getAllowedTypes($scope.content.id);
        
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
                $scope.listViewResultSet = contentFactory.getChildren($scope.content.id, $scope.options);
                
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
angular.module('umbraco').controller("Umbraco.Editors.MediaPickerController", function($rootScope, $scope, dialog, $log){
    $scope.openMediaPicker =function(value){
            var d = dialog.mediaPicker({scope: $scope, callback: populate});
    };

    function populate(data){
    	$log.log(data.selection);
        $scope.model.value = data.selection;    
    }
});
angular.module("umbraco")
    .controller("Umbraco.Editors.RTEController", 
    function($rootScope, $scope, dialog, $log){
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
                                dialog.mediaPicker({scope: $scope, callback: function(data){
                                 
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
angular.module("umbraco").controller("Umbraco.Editors.TagsController", 
	function($rootScope, $scope, dialog, $log, tagsFactory) {	
		
		require( 
		[
			'/belle/views/propertyeditors/umbraco/tags/bootstrap-tags.custom.js',
			'css!/belle/views/propertyeditors/umbraco/tags/bootstrap-tags.custom.css'
		],function(){
		
			// Get data from tagsFactory
			$scope.tags = tagsFactory.getTags("group");

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

return angular;
});