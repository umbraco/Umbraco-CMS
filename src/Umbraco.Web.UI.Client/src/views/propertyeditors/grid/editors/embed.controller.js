angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.EmbedController",
    function ($scope, $timeout, $sce, editorService) {

    	$scope.hasEmbed = function(){
            return $scope.control.value !== null;
        }
    	$scope.getEmbed = function(){
            return $sce.trustAsHtml($scope.control.value);
        }
    	$scope.setEmbed = function(){
            var embed = {
                submit: function(model) {
                    $scope.control.value = model.embed.preview;
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };
            editorService.embed(embed);
        };
        
});
