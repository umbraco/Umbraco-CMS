function startUpVideosDashboardController($scope, xmlhelper, $log, $http) {
    $scope.videos = [];
    $scope.init = function(url){
        var proxyUrl = "dashboard/feedproxy.aspx?url=" + url;
        $http.get(proxyUrl).then(function(data){
              var feed = $(data.data);
              $('item', feed).each(function (i, item) {
                  var video = {};
                  video.thumbnail = $(item).find('thumbnail').attr('url');
                  video.title = $("title", item).text();
                  video.link = $("guid", item).text();
                  $scope.videos.push(video);
              });
        });
    };
}

angular.module("umbraco").controller("Umbraco.Dashboard.StartupVideosController", startUpVideosDashboardController);


function startUpDynamicContentController(dashboardResource, assetsService, tourService) {
    var vm = this;
    vm.loading = true;
    vm.showDefault = false;
    
    vm.startTour = startTour;
    vm.startTourTwo = startTourTwo;
    vm.startTourThree = startTourThree;
    vm.startTourFour = startTourFour;
    vm.startTourSeven = startTourSeven;

    function startTour() {

        var tour = {
            "options": {
                "name": "Create document type",
                "alias": "umbIntroCreateDocType"
            },
            "steps": [
                {
                    element: "#applications [data-element='section-settings']",
                    title: "Navigate to the settings sections",
                    content: "In the settings section we will find the document types",
                    event: "click"
                },
                {
                    element: "#tree [data-element='tree-item-documentTypes']",
                    title: "Create document type",
                    content: "<p>Hover the document types tree and click the <b>three small dots</b> to open the context menu</p>",
                    event: "click",
                    clickElement: "#tree [data-element='tree-item-documentTypes'] [data-element='tree-item-options']"
                },
                {
                    element: "#dialog [data-element='action-documentType']",
                    title: "Create document type",
                    content: "Click <b>Document Type</b> to create a new document type with a template",
                    event: "click"
                },
                {
                    element: "[data-element='editor-name-field']",
                    title: "Enter a name",
                    content: "<p>Our document type needs a name. Enter <b>Home</b> in the field and click <i>Next</i></p>"
                },
                {
                    element: "[data-element='editor-description']",
                    title: "Enter a description",
                    content: "<p>A description helps the content editor pick the right document type when creating content:<br/><pre>The home to our website</pre></p>"
                },
                {
                    element: "[data-element='group-add']",
                    title: "Add tab",
                    content: "Tabs help us organize the content on a content page. Click <b>Add new tab</b> to add a tab.",
                    event: "click"
                },
                {
                    element: "[data-element='group-name']",
                    title: "Enter a name",
                    content: "Enter <b>Content</b> in the tab name"
                },
                {
                    element: "[data-element='property-add']",
                    title: "Add a property",
                    content: "<p>Properties are the different types of data on our content page.</p><p>On our Home page we wan't to add a welcome text.</p><p>Click <b>Add property</b> to open the property dialog</p>",
                    event: "click"
                },
                {
                    element: "[data-element~='overlay-property-settings'] [data-element='property-name']",
                    title: "Enter a name",
                    content: "Enter <b>Welcome Text</b> as name for the property"
                },
                {
                    element: "[data-element~='overlay-property-settings'] [data-element='property-description']",
                    title: "Enter a description",
                    content: "Enter a description for the property editor:<br/> <pre>Write a nice introduction text so the visitors feel welcome</pre>"
                },
                {
                    element: "[data-element~='overlay-property-settings'] [data-element='editor-add']",
                    title: "Add editor",
                    content: "The editor define what data type the property is. Click <b>Add editor</b> to open the editor picker dialog",
                    event: "click"
                },
                {
                    element: "[data-element~='overlay-editor-picker']",
                    title: "Editor picker",
                    content: "<p>In the editor picker dialog we can pick one of the many build in editor.</p>"
                },
                {
                    element: "[data-element~='overlay-editor-picker'] [data-element='editor-Umbraco.TextboxMultiple']",
                    title: "Select editor",
                    content: "Select the <b>Textarea</b> editor which allows the content editor to enter long texts",
                    event: "click"
                },
                {
                    element: "[data-element~='overlay-editor-settings']",
                    title: "Editor settings",
                    content: "Each editor can have individual settings. We don't need to change any of these now."
                },
                {
                    element: "[data-element~='overlay-editor-settings'] [data-element='overlay-submit']",
                    title: "Save editor",
                    content: "Click <b>Submit</b> to save the editor and any changes you may have made to the editor settings.",
                    event: "click"
                },
                {
                    element: "[data-element~='overlay-property-settings'] [data-element='overlay-submit']",
                    title: "Save property",
                    content: "Click <b>Submit</b> to add the property",
                    event: "click"
                },
                {
                    element: "[data-element='button-group-primary']",
                    title: "Save the document type",
                    content: "Click <b>Save</b> to create your document type",
                    event: "click"
                }
            ]
        };

        tourService.startTour(tour);
        
    }

    function startTourTwo() {

        var tour = {
            "options": {
                "name": "Create Content",
                "alias": "umbIntroCreateContent"
            },
            "steps": [
                {
                    element: "[data-element='tree-root']",
                    title: "Open context menu",
                    content: "<p>Open the context menu by hovering the root of the content section.</p><p>Now click the <b>three small dots</b> to the right</p>",
                    event: "click",
                    clickElement: "[data-element='tree-root'] [data-element='tree-item-options']"
                },
                {
                    element: "[data-element='action-create-home']",
                    title: "Create Home page",
                    content: "<p>Click on <b>Home</b> to create a new page of type <i>Home</i></p>",
                    event: "click"
                },
                {
                    element: "[data-element='editor-content'] [data-element='editor-name-field']",
                    title: "Give your new page a name",
                    content: "<p>Our new page needs a name. Enter <b>Home</b> in the field and click <b>Next</b></p>"
                },
                {
                    element: "[data-element='editor-content'] [data-element='property-welcomeText']",
                    title: "Add a welcome text",
                    content: "<p>Add content to the <b>Welcome Text</b> field</p><p>If you don't have any ideas here is a start:<br/> <pre>I am learning Umbraco. High Five I Rock #H5IR</pre></p>"
                },
                {
                    element: "[data-element='editor-content'] [data-element='button-group-primary']",
                    title: "Save and publish",
                    content: "<p>Now click the <b>Save and publish</b> button to save and publish your changes</p>",
                    event: "click"
                }
            ]
        };

        tourService.startTour(tour);
    }

    function startTourThree() {

        var tour = {
            "options": {
                "name": "Render in template",
                "alias": "umbIntroRenderInTemplate"
            },
            "steps": [
                {
                    element: "#applications [data-element='section-settings']",
                    title: "Navigate to the settings section",
                    content: "In the <b>Settings</b> section you will find the templates for all your document types",
                    event: "click"
                },
                {
                    element: "#tree [data-element='tree-item-templates']",
                    title: "Expand the Templates node",
                    content: "<p>To see all our templates click the <b>small triangle</b> to the left of the templates node</p>",
                    event: "click",
                    clickElement: "#tree [data-element='tree-item-templates'] [data-element='tree-item-expand']"
                },
                {
                    element: "#tree [data-element='tree-item-templates'] [data-element='tree-item-Home']",
                    title: "Open Home template",
                    content: "<p>Click the <b>Home</b> template to open and edit it</p>",
                    event: "click"
                },
                {
                    element: "[data-element='editor-templates'] [data-element='code-editor']",
                    title: "Edit template",
                    content: '<p>Templates can be edited here or in your favorite code editor.</p><p>To render the value we entered on the <b>Home</b> page add the following to the template:<br/> <pre>@Model.Content.GetPropertyValue("welcomeText")</pre></p>'
                },
                {
                    element: "[data-element='editor-templates'] [data-element='button-save']",
                    title: "Save the template",
                    content: "Click the save button and your template will be saved",
                    event: "click"
                }
            ]
        };

        tourService.startTour(tour);

    }

    function startTourFour() {
        
        var tour = {
            "options": {
                "name": "View Home page",
                "alias": "umbIntroViewHomePage"
            },
            "steps": [
                {
                    element: "#tree [data-element='tree-item-Home']",
                    title: "Open the Home page",
                    content: "<p>Click the <b>Home</b> page to open it</p>",
                    event: "click"
                },
                {
                    element: "[data-element='editor-content'] [data-element='tab-Generic properties']",
                    title: "Properties",
                    content: "<p>Under the properties tab you will find default information about the content item</p>",
                    event: "click"
                },
                {
                    element: "[data-element='editor-content'] [data-element='property-_umb_urls']",
                    title: "Open page",
                    content: "<p>Click the <b>Link</b> to view your page.</p><p>Tip: Click the preview button in the bottom right corner to preview changes without publishing them</p>",
                    event: "click",
                    clickElement: "[data-element='editor-content'] [data-element='property-_umb_urls'] a[target='_blank']"
                }
            ]
        };

        tourService.startTour(tour);

    }

    function startTourSeven() {

        var tour = {
            "options": {
                "name": "The media library",
                "alias": "umbIntroMediaSection"
            },
            "steps": [
                {
                    element: "#applications [data-element='section-media']",
                    title: "Navigate to the media section",
                    content: "The media section is where you will manage all your media items",
                    event: "click"
                },
                {
                    element: "#tree [data-element='tree-root']",
                    title: "Create a new folder",
                    content: "<p>Hover the media root and click the <b>three small dots</b> on the right side of the item</p>",
                    event: "click",
                    clickElement: "#tree [data-element='tree-root'] [data-element='tree-item-options']"
                },
                {
                    element: "#dialog [data-element='action-Folder']",
                    title: "Create a new folder",
                    content: "<p>Select the <b>Folder</b> options to create a new folder</p>",
                    event: "click"
                },
                {
                    element: "[data-element='editor-media'] [data-element='editor-name-field']",
                    title: "Enter a name",
                    content: "<p>Enter <b>'My folder'</b> in the field</p>"
                },
                {
                    element: "[data-element='editor-media'] [data-element='button-save']",
                    title: "Save the folder",
                    content: "<p>Click the <b>Save</b> button to create the new folder</p>",
                    event: "click"
                },
                {
                    element: "[data-element='editor-media'] [data-element='dropzone']",
                    title: "Upload images",
                    content: "<p>In the upload area you can upload your media items.</p><p>Click the <b>Upload button</b> and select some images on your computer and upload them.</p>",
                    event: "click",
                    clickElement: "[data-element='editor-media'] [data-element='button-upload-media']"
                },
                {
                    element: "[data-element='editor-media'] [data-element='media-grid-item-0']",
                    title: "View media item details",
                    content: "Hover the media item and <b>Click the purple bar</b> to view details about the media item",
                    event: "click",
                    clickElement: "[data-element='editor-media'] [data-element='media-grid-item-0'] [data-element='media-grid-item-edit']"
                },
                {
                    element: "[data-element='editor-media'] [data-element='property-umbracoFile']",
                    title: "The uploaded image",
                    content: "<p>Here you can see the image you have uploaded.</p><p>Use the dot in the center of the image to set a focal point on the image.</p>"
                },
                {
                    element: "[data-element='editor-media'] [data-element='property-umbracoBytes']",
                    title: "Image size",
                    content: "<p>You will also find other details about the image, like the size</p><p>You can add extra properties to an image by creating or editing the <b>Media types</b></p>"
                },
                {
                    element: "[data-element='editor-media'] [data-element='tab-Generic properties']",
                    title: "Properties",
                    content: "Like the content section you can also find default properties about the media item. You will find these under the properties tab",
                    event: "click"
                },
                {
                    element: "[data-element='editor-media'] [data-element='property-_umb_urls']",
                    title: "Link to media",
                    content: "The path to the media item..."
                },
                {
                    element: "[data-element='editor-media'] [data-element='property-_umb_updatedate']",
                    title: "Last edited",
                    content: "...and information about when the item has been created and edited."
                }
            ]
        };

        tourService.startTour(tour);

    }

    // default dashboard content
    vm.defaultDashboard = {
        infoBoxes: [
            {
                title: "Documentation",
                description: "Find the answers to your Umbraco questions",
                url: "https://our.umbraco.org/documentation/?utm_source=core&utm_medium=dashboard&utm_content=text&utm_campaign=documentation/"
            },
            {
                title: "Community",
                description: "Find the answers or ask your Umbraco questions",
                url: "https://our.umbraco.org/?utm_source=core&utm_medium=dashboard&utm_content=text&utm_campaign=our_forum"
            },
            {
                title: "Umbraco.tv",
                description: "Tutorial videos (some are free, some are on subscription)",
                url: "https://umbraco.tv/?utm_source=core&utm_medium=dashboard&utm_content=text&utm_campaign=tutorial_videos"
            },
            {
                title: "Training",
                description: "Real-life training and official Umbraco certifications",
                url: "https://umbraco.com/training/?utm_source=core&utm_medium=dashboard&utm_content=text&utm_campaign=training"
            }
        ],
        articles: [
            {
                title: "Umbraco.TV - Learn from the source!",
                description: "Umbraco.TV will help you go from zero to Umbraco hero at a pace that suits you. Our easy to follow online training videos will give you the fundamental knowledge to start building awesome Umbraco websites.",
                img: "views/dashboard/default/umbracotv.jpg",
                url: "https://umbraco.tv/?utm_source=core&utm_medium=dashboard&utm_content=image&utm_campaign=tv",
                altText: "Umbraco.TV - Hours of Umbraco Video Tutorials",
                buttonText: "Visit Umbraco.TV"
            },
            {
                title: "Our Umbraco - The Friendliest Community",
                description: "Our Umbraco - the official community site is your one stop for everything Umbraco. Whether you need a question answered or looking for cool plugins, the world's best and friendliest community is just a click away.",
                img: "views/dashboard/default/ourumbraco.jpg",
                url: "https://our.umbraco.org/?utm_source=core&utm_medium=dashboard&utm_content=image&utm_campaign=our",
                altText: "Our Umbraco",
                buttonText: "Visit Our Umbraco"
            }
        ]
    };

    
    //proxy remote css through the local server
    assetsService.loadCss( dashboardResource.getRemoteDashboardCssUrl("content") );
    dashboardResource.getRemoteDashboardContent("content").then(
        function (data) {

            vm.loading = false;

            //test if we have received valid data
            //we capture it like this, so we avoid UI errors - which automatically triggers ui based on http response code
            if (data && data.sections) {
                vm.dashboard = data;
            } else{
                vm.showDefault = true;
            }

        },

        function (exception) {
            console.error(exception);
            vm.loading = false;
            vm.showDefault = true;
        });
}

angular.module("umbraco").controller("Umbraco.Dashboard.StartUpDynamicContentController", startUpDynamicContentController);


function FormsController($scope, $route, $cookieStore, packageResource, localizationService) {
    $scope.installForms = function(){
        $scope.state = localizationService.localize("packager_installStateDownloading");
        packageResource
            .fetch("CD44CF39-3D71-4C19-B6EE-948E1FAF0525")
            .then(function(pack) {
                $scope.state = localizationService.localize("packager_installStateImporting");
                    return packageResource.import(pack);
                },
                $scope.error)
            .then(function(pack) {
                $scope.state = localizationService.localize("packager_installStateInstalling");
                    return packageResource.installFiles(pack);
                },
                $scope.error)
            .then(function(pack) {
                $scope.state = localizationService.localize("packager_installStateRestarting");
                    return packageResource.installData(pack);
                },
                $scope.error)
            .then(function(pack) {
                $scope.state = localizationService.localize("packager_installStateComplete");
                    return packageResource.cleanUp(pack);
                },
                $scope.error)
            .then($scope.complete, $scope.error);
    };

    $scope.complete = function(result){
        var url = window.location.href + "?init=true";
        $cookieStore.put("umbPackageInstallId", result.packageGuid);
        window.location.reload(true);
    };

    $scope.error = function(err){
        $scope.state = undefined;
        $scope.error = err;
        //This will return a rejection meaning that the promise change above will stop
        return $q.reject();
    };


    function Video_player (videoId) {
      // Get dom elements
      this.container      = document.getElementById(videoId);
      this.video          = this.container.getElementsByTagName('video')[0];

      //Create controls
      this.controls = document.createElement('div');
      this.controls.className="video-controls";

      this.seek_bar = document.createElement('input');
      this.seek_bar.className="seek-bar";
      this.seek_bar.type="range";
      this.seek_bar.setAttribute('value', '0');

      this.loader = document.createElement('div');
      this.loader.className="loader";

      this.progress_bar = document.createElement('span');
      this.progress_bar.className="progress-bar";

      // Insert controls
      this.controls.appendChild(this.seek_bar);
      this.container.appendChild(this.controls);
      this.controls.appendChild(this.loader);
      this.loader.appendChild(this.progress_bar);
    }


    Video_player.prototype
      .seeking = function() {
        // get the value of the seekbar (hidden input[type="range"])
        var time = this.video.duration * (this.seek_bar.value / 100);

        // Update video to seekbar value
        this.video.currentTime = time;
      };

    // Stop video when user initiates seeking
    Video_player.prototype
      .start_seek = function() {
        this.video.pause();
      };

    // Start video when user stops seeking
    Video_player.prototype
      .stop_seek = function() {
        this.video.play();
      };

    // Update the progressbar (span.loader) according to video.currentTime
    Video_player.prototype
      .update_progress_bar = function() {
        // Get video progress in %
        var value = (100 / this.video.duration) * this.video.currentTime;

        // Update progressbar
        this.progress_bar.style.width = value + '%';
      };

    // Bind progressbar to mouse when seeking
    Video_player.prototype
      .handle_mouse_move = function(event) {
        // Get position of progressbar relative to browser window
        var pos = this.progress_bar.getBoundingClientRect().left;

        // Make sure event is reckonized cross-browser
        event = event || window.event;

        // Update progressbar
        this.progress_bar.style.width = (event.clientX - pos) + "px";
      };

    // Eventlisteners for seeking
    Video_player.prototype
      .video_event_handler = function(videoPlayer, interval) {
        // Update the progress bar
        var animate_progress_bar = setInterval(function () {
              videoPlayer.update_progress_bar();
            }, interval);

        // Fire when input value changes (user seeking)
        videoPlayer.seek_bar
          .addEventListener("change", function() {
              videoPlayer.seeking();
          });

        // Fire when user clicks on seekbar
        videoPlayer.seek_bar
          .addEventListener("mousedown", function (clickEvent) {
              // Pause video playback
              videoPlayer.start_seek();

              // Stop updating progressbar according to video progress
              clearInterval(animate_progress_bar);

              // Update progressbar to where user clicks
              videoPlayer.handle_mouse_move(clickEvent);

              // Bind progressbar to cursor
              window.onmousemove = function(moveEvent){
                videoPlayer.handle_mouse_move(moveEvent);
              };
          });

        // Fire when user releases seekbar
        videoPlayer.seek_bar
          .addEventListener("mouseup", function () {

              // Unbind progressbar from cursor
              window.onmousemove = null;

              // Start video playback
              videoPlayer.stop_seek();

              // Animate the progressbar
              animate_progress_bar = setInterval(function () {
                  videoPlayer.update_progress_bar();
              }, interval);
          });
      };


    var videoPlayer = new Video_player('video_1');
    videoPlayer.video_event_handler(videoPlayer, 17);
}

angular.module("umbraco").controller("Umbraco.Dashboard.FormsDashboardController", FormsController);

function startupLatestEditsController($scope) {

}
angular.module("umbraco").controller("Umbraco.Dashboard.StartupLatestEditsController", startupLatestEditsController);

function MediaFolderBrowserDashboardController($rootScope, $scope, $location, contentTypeResource, userService) {

    var currentUser = {};

    userService.getCurrentUser().then(function (user) {

        currentUser = user;

        // check if the user has access to the root which they will require to see this dashboard
        if (currentUser.startMediaIds.indexOf(-1) >= 0) {

            //get the system media listview
            contentTypeResource.getPropertyTypeScaffold(-96)
                .then(function(dt) {

                    $scope.fakeProperty = {
                        alias: "contents",
                        config: dt.config,
                        description: "",
                        editor: dt.editor,
                        hideLabel: true,
                        id: 1,
                        label: "Contents:",
                        validation: {
                            mandatory: false,
                            pattern: null
                        },
                        value: "",
                        view: dt.view
                    };

            });

        } else if (currentUser.startMediaIds.length > 0){
            // redirect to start node
            $location.path("/media/media/edit/" + (currentUser.startMediaIds.length === 0 ? -1 : currentUser.startMediaIds[0]));
        }

    });

}
angular.module("umbraco").controller("Umbraco.Dashboard.MediaFolderBrowserDashboardController", MediaFolderBrowserDashboardController);
