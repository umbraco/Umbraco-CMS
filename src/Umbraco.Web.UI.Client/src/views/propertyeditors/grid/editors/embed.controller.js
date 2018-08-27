angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.EmbedController",
    function ($scope, $timeout, $sce, editorService) {

        function onInit() {
            $scope.trustedValue = null;
            $scope.trustedValue = $sce.trustAsHtml($scope.control.value);

            if(!$scope.control.value) {
                $timeout(function(){
                    if($scope.control.$initializing){
                        $scope.setEmbed();
                    }
                }, 200);
            }
        }

    	$scope.setEmbed = function(){
            var embed = {
                submit: function(model) {
                    $scope.control.value = model.embed.preview;
                    $scope.trustedValue = $sce.trustAsHtml(model.embed.preview);
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };
            editorService.embed(embed);
        };

        onInit();
});
