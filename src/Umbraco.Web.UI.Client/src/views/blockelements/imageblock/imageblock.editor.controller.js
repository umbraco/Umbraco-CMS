
(function () {
    'use strict';

    function ImageBlockEditor($scope, entityResource) {

        const bc = this;

        var firstProperty = $scope.block.content.variants[0].tabs[0].properties[0];

        entityResource.getById(firstProperty.value, "Media").then(function(ent) {
            console.log(ent)
            bc.imageUrl = ent.metaData.MediaPath;
        });

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.ImageBlockEditor", ImageBlockEditor);

})();
        