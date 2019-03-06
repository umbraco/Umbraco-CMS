angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.EmbedController",
    function ($scope, $timeout, $sce, editorService) {

        function onInit() {

            console.log("$scope.control", $scope.control);

            console.log("value", $scope.control.value);
            console.log("is object", angular.isObject($scope.control.value));

            console.log("new syntax", $scope.control.value && $scope.control.value.embed && $scope.control.value.embed.preview);

            var embedPreview = angular.isObject($scope.control.value) && $scope.control.value.preview ? $scope.control.value.preview : $scope.control.value;
            console.log("embedPreview", embedPreview);

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

            console.log("setEmbed", $scope.control.value);

            var original = angular.isObject($scope.control.value) ? $scope.control.value : null;

            var embed = {
                original: original,
                submit: function (model) {
                    console.log("model", model);

                    //$scope.control.value = model.embed.preview;
                    //$scope.trustedValue = $sce.trustAsHtml(model.embed.preview);

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
