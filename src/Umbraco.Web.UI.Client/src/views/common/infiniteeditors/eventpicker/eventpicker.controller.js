(function () {
  "use strict";

  function LanguagePickerController($scope, languageResource, localizationService, webhooksResource) {

    var vm = this;

    vm.events = [];
    vm.loading = false;

    vm.selectEvent = selectEvent;
    vm.submit = submit;
    vm.close = close;

    function onInit() {

      vm.loading = true;

      // set default title
      if (!$scope.model.title) {
        localizationService.localize("defaultdialogs_selectLanguages").then(function (value) {
          $scope.model.title = value;
        });
      }

      // make sure we can push to something
      if (!$scope.model.selection) {
        $scope.model.selection = [];
      }

      getAllEvents();
      vm.loading = false;
    }

    function getAllEvents(){
      // get all events
      webhooksResource.getAllEvents()
        .then((data) => {
          data.forEach(function (event) {
            let eventObject = { name: event, selected: false}
            vm.events.push(eventObject);
          });
        });
    }

    function selectEvent(event) {
      if (!event.selected) {
        event.selected = true;
        $scope.model.selection.push(event);
        if(event.name.toLowerCase().includes("content")){
          vm.events = vm.events.filter(event => event.name.toLowerCase().includes("content"));
        }
        else{
          vm.events = vm.events.filter(event => event.name.toLowerCase().includes("media"));
        }

      } else {

        $scope.model.selection.forEach(function (selectedEvent, index) {
          if (selectedEvent.name === event.name) {
            event.selected = false;
            $scope.model.selection.splice(index, 1);
          }
        });

        if($scope.model.selection.length === 0){
          vm.events = [];
          getAllEvents();
        }
      }
    }

    function submit(model) {
      if ($scope.model.submit) {
        $scope.model.selection = $scope.model.selection.map((item) => item.name)
        $scope.model.submit(model);
      }
    }

    function close() {
      if ($scope.model.close) {
        $scope.model.close();
      }
    }

    onInit();

  }

  angular.module("umbraco").controller("Umbraco.Editors.EventPickerController", LanguagePickerController);

})();
