angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.EmbedController",
    function ($scope, $timeout, $sce, editorService) {
        
        
        
    	function getEmbed() {
            return $sce.trustAsHtml($scope.control.value);
        }
        
        
        $scope.embedHtml = getEmbed();
        $scope.$watch('control.value', function(newValue, oldValue) {
            if(angular.equals(newValue, oldValue)){
                return; // simply skip that
            }
            
            $scope.embedHtml = getEmbed();
        }, false);
    	$scope.setEmbed = function() {
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
