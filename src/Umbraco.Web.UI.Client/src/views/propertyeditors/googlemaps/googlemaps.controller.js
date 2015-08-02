angular.module("umbraco")
.controller("Umbraco.PropertyEditors.GoogleMapsController",
    function ($element, $rootScope, $scope, notificationsService, dialogService, assetsService, $log, $timeout) {

        assetsService.loadJs('http://www.google.com/jsapi')
            .then(function () {
                google.load("maps", "3",
                            {
                                callback: initMap,
                                other_params: "sensor=false"
                            });
            });

        function initMap() {
            //Google maps is available and all components are ready to use.
            var valueArray = $scope.model.value.split(',');
            var latLng = new google.maps.LatLng(valueArray[0], valueArray[1]);
            var mapDiv = document.getElementById($scope.model.alias + '_map');
            var mapOptions = {
                zoom: $scope.model.config.zoom,
                center: latLng,
                mapTypeId: google.maps.MapTypeId[$scope.model.config.mapType]
            };
            var geocoder = new google.maps.Geocoder();
            var map = new google.maps.Map(mapDiv, mapOptions);

            var marker = new google.maps.Marker({
                map: map,
                position: latLng,
                draggable: true
            });

            google.maps.event.addListener(map, 'click', function (event) {

                dialogService.mediaPicker({
                    callback: function (data) {
                        var image = data.selection[0].src;

                        var latLng = event.latLng;
                        var marker = new google.maps.Marker({
                            map: map,
                            icon: image,
                            position: latLng,
                            draggable: true
                        });

                        google.maps.event.addListener(marker, "dragend", function (e) {
                            var newLat = marker.getPosition().lat();
                            var newLng = marker.getPosition().lng();

                            codeLatLng(marker.getPosition(), geocoder);

                            //set the model value
                            $scope.model.vvalue = newLat + "," + newLng;
                        });

                    }
                });
            });

            var tabShown = function(e) {
                google.maps.event.trigger(map, 'resize');
            };

            //listen for tab changes
            if (tabsCtrl != null) {
                tabsCtrl.onTabShown(function (args) {
                    tabShown();
                });
            }

            $element.closest('.umb-panel.tabbable').on('shown', '.nav-tabs a', tabShown);

            $scope.$on('$destroy', function () {
                $element.closest('.umb-panel.tabbable').off('shown', '.nav-tabs a', tabShown);
            });
        }

        function codeLatLng(latLng, geocoder) {
            geocoder.geocode({ 'latLng': latLng },
                function (results, status) {
                    if (status == google.maps.GeocoderStatus.OK) {
                        var location = results[0].formatted_address;
                        $rootScope.$apply(function () {
                            notificationsService.success("Peter just went to: ", location);
                        });
                    }
                });
        }

        //here we declare a special method which will be called whenever the value has changed from the server
        //this is instead of doing a watch on the model.value = faster
        $scope.model.onValueChanged = function (newVal, oldVal) {
            //update the display val again if it has changed from the server
            initMap();
        };
    });