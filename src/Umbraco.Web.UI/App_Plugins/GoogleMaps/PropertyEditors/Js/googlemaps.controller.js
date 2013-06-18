define(['app', 'angular'], function (app, angular) {

angular.module("umbraco")
.controller("Umbraco.Editors.GoogleMapsController",
    function ($rootScope, $scope, notificationsService, dialogService) {

    require(
        [
            'async!http://maps.google.com/maps/api/js?sensor=false'
        ],
        function () {

            
            //Google maps is available and all components are ready to use.
            var geocoder = new google.maps.Geocoder();
            var latLng = new google.maps.LatLng(-34.397, 150.644);
            var mapDiv = document.getElementById($scope.model.alias + '_map');
            var mapOptions = {
                zoom: 2,
                center: latLng,
                mapTypeId: google.maps.MapTypeId.ROADMAP
            };
            
            //lets load the map
            var map = new google.maps.Map(mapDiv, mapOptions);
            


                          
            //lets add a picture on click
            google.maps.event.addListener(map, 'click', function(event) {
    
                //opens the media dialog
                dialogService.mediaPicker({scope: $scope, callback: function(data){
                    var image = data.selection[0].thumbnail;
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
                        
                        //find the location
                        codeLatLng(marker.getPosition());

                        //set the model value
                        $scope.model.value = newLat + "," + newLng;
                    });

                }});
            });
                    



            function codeLatLng(latLng) {
                geocoder.geocode({'latLng': latLng},
                    function(results, status) {
                        if (status == google.maps.GeocoderStatus.OK) {
                            var location = results[0].formatted_address;
                                $rootScope.$apply(function () {

                                    //alert(location);    

                                    notificationsService.success("Pete just went to: ", location);
                                });
                        }
                    });
            }



            $('a[data-toggle="tab"]').on('shown', function (e) {
                google.maps.event.trigger(map, 'resize');
            });
        }
    );    
});

return angular;

});