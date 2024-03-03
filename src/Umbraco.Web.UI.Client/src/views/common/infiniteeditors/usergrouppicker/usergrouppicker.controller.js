(function () {
  "use strict";

  function UserGroupPickerController($scope, userGroupsResource, localizationService) {

    var vm = this;

    vm.userGroups = [];
    vm.loading = false;

    vm.filter = "";

    vm.pagination = {
      pageNumber: 1,
      totalPages: 1
    };

    vm.selectUserGroup = selectUserGroup;
    vm.submit = submit;
    vm.close = close;
    vm.nextPage = nextPage;
    vm.prevPage = prevPage;
    vm.setPage = setPage;
    vm.searchChanged = searchChanged;

    //////////

    function onInit() {

      vm.loading = true;

      // set default title
      if (!$scope.model.title) {
        localizationService.localize("user_selectUserGroups").then(function (value) {
          $scope.model.title = value;
        });
      }

      // make sure we can push to something
      if (!$scope.model.selection) {
        $scope.model.selection = [];
      }

      // get venues
      getGroups();
      //userGroupsResource.getUserGroups().then(function (userGroups) {
      //  vm.userGroups = userGroups;

      //  if ($scope.model.selection && $scope.model.selection.length > 0) {
      //    preSelect($scope.model.selection);
      //  }

      //  vm.loading = false;

      //});

    }

    function nextPage() {
      vm.pagination.pageNumber = vm.pagination.pageNumber + 1;
      getGroups();
    }
    function prevPage() {
      vm.pagination.pageNumber = vm.pagination.pageNumber - 1;
      getGroups();
    }

    function setPage(pageNumber) {
      vm.pagination.pageNumber = pageNumber;
      getGroups();
    }

    function searchChanged(searchTerm) {
      vm.filter = searchTerm;
      getGroups();
    }

    function getGroups() {
      userGroupsResource.getPagedUserGroups({
        onlyCurrentUserGroups: true,
        page: vm.pagination.pageNumber,
        searchTerm: vm.filter
      }).then(function (data) {
        vm.pagination.totalPages = data.totalPages;
        vm.userGroups = data.groups;
        preSelect($scope.model.selection);
        vm.loading = false;
      });
    }
    function preSelect(selection) {

      selection.forEach(function (selected) {

        vm.userGroups.forEach(function (userGroup) {
          if (selected.id === userGroup.id) {
            userGroup.selected = true;
          }
        });

      });
    }

    function selectUserGroup(userGroup) {

      if (!userGroup.selected) {

        userGroup.selected = true;
        $scope.model.selection.push(userGroup);

      } else {

        $scope.model.selection.forEach(function (selectedUserGroup, index) {
          if (selectedUserGroup.id === userGroup.id) {
            userGroup.selected = false;
            $scope.model.selection.splice(index, 1);
          }
        });

      }

    }

    function submit(model) {
      if ($scope.model.submit) {
        $scope.model.submit(model);
      }
    }

    function close() {
      if ($scope.model.close) {
        $scope.model.close();
      }
    }

    onInit();

    var unbindFilterWatcher = $scope.$watch("vm.filter", function (newVal, oldVal) {
      vm.pagination.pageNumber = 1;
      getGroups();
    });

    $scope.$on("$destroy", function () {
      unbindFilterWatcher();
    });

  }

  angular.module("umbraco").controller("Umbraco.Editors.UserGroupPickerController", UserGroupPickerController);

})();
