angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.EmbedController",
    function ($scope, $timeout, $sce, editorService) {

        function onInit() {

            $scope.control.icon = $scope.control.icon || 'icon-movie-alt';

            var embedPreview = Utilities.isObject($scope.control.value) && $scope.control.value.preview ? $scope.control.value.preview : $scope.control.value;

            $scope.trustedValue = embedPreview ? $sce.trustAsHtml(embedPreview) : null;

            if(!$scope.control.value) {
                $timeout(function(){
                    if($scope.control.$initializing){
                        $scope.setEmbed();
                    }
                }, 200);
            }
        }

        $scope.setEmbed = function () {

            var modify = Utilities.isObject($scope.control.value) ? $scope.control.value : null;

            var embed = {
                modify: modify,
                submit: function (model) {

                    var embed = {
                        constrain: model.embed.constrain,
                        height: model.embed.height,
                        width: model.embed.width,
                        url: model.embed.url,
                        info: model.embed.info,
                        preview: model.embed.preview
                    };

                    $scope.control.value = embed;
                    $scope.trustedValue = $sce.trustAsHtml(embed.preview);

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
