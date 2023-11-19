function HotspotController($scope, entityResource) {

    const vm = this;

    vm.clear = clear;
    vm.focalPointChanged = focalPointChanged;
    vm.imageLoaded = imageLoaded;

    //setup the default config
    var config = {
        mediaId: null
    };
    
    // map the user config
    Utilities.extend(config, $scope.model.config);

    // map back to the model
    $scope.model.config = config;

    function init() {

      setModelValueWithSrc($scope.model.value);

      retrieveMedia();
    }

    function retrieveMedia() {
        
      var id = $scope.model.config.mediaId || null;
      console.log("id", id);
        
        if (id == null) {
            return;
        }
        
      entityResource.getById(id, "Media").then(media => {
            console.log("media", media);
            $scope.media = media;
            $scope.imageSrc = media.metaData.MediaPath;
        });
    }

    function clear() {
      focalPointChanged(null, null);
    }

    /**
    * Used to assign a new model value
    * @param {any} src
    */
    function setModelValueWithSrc(src) {
      if (!$scope.model.value || !$scope.model.value.src) {
        //we are copying to not overwrite the original config
        $scope.model.value = Utilities.extend(Utilities.copy($scope.model.config), { src: src });
      }
    }

    /**
    * Called when the umbImageGravity component updates the focal point value
    * @param {any} left
    * @param {any} top
    */
    function focalPointChanged(left, top) {
        console.log("focalPointChanged", left, top);

        if (left === null && top === null) {
            $scope.model.value.focalPoint = null;
        }
        else {
            //update the model focalpoint value
            $scope.model.value.focalPoint = {
              left: left,
              top: top
            };
        }

        //set form to dirty to track changes
        //setDirty();
    }

    function imageLoaded(isCroppable, hasDimensions) {
        $scope.isCroppable = isCroppable;
        $scope.hasDimensions = hasDimensions;
    }

    function setDirty() {
      if ($scope.imageCropperForm) {
        $scope.imageCropperForm.modelValue.$setDirty();
      }
    }

    init();

}

angular.module("umbraco").controller("Umbraco.PropertyEditors.HotspotController", HotspotController);
