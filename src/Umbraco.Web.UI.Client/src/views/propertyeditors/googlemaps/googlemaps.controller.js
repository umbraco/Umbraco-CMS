angular.module("umbraco")
.controller("Umbraco.Editors.GoogleMapsController",
    function ($rootScope, $scope, notificationsService, dialogService, assetsService, $log, $timeout) {
    
    assetsService.loadJs('http://www.google.com/jsapi')
        .then(function(){
            google.load("maps", "3",
                        {
                            callback: initMap,
                            other_params: "sensor=false"
                        });
    });

    function initMap(){
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

        google.maps.event.addListener(map, 'click', function(event) {

            dialogService.mediaPicker({scope: $scope, callback: function(data){
                var image = data.selection[0].src;

                var latLng = event.latLng;
                var marker = new google.maps.Marker({
                    map: map,
                    icon: image,
                    position: latLng,
                    draggable: true
                });

                google.maps.event.addListener(marker, "dragend", function(e){
                    var newLat = marker.getPosition().lat();
                    var newLng = marker.getPosition().lng();

                    codeLatLng(marker.getPosition(), geocoder);

                    //set the model value
                    $scope.model.vvalue = newLat + "," + newLng;
                });

            }});
        });

        $('a[data-toggle="tab"]').on('shown', function (e) {
            google.maps.event.trigger(map, 'resize');
        });
    }

    function codeLatLng(latLng, geocoder) {
        geocoder.geocode({'latLng': latLng},
            function(results, status) {
                if (status == google.maps.GeocoderStatus.OK) {
                    var location = results[0].formatted_address;
                        $rootScope.$apply(function () {
                            notificationsService.success("Peter just went to: ", location);
                        });
                }
            });
    }
});