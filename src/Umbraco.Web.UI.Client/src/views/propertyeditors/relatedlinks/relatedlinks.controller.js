angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.RelatedLinksController",
        function ($rootScope, $scope, dialogService) {

            $scope.newCaption = '';
            $scope.newLink = 'http://';
            $scope.newNewWindow = false;
            $scope.newInternal = null;
            $scope.newInternalName = '';
            $scope.addExternal = true;
            
            $scope.relatedLinks = [
                { caption: 'Google', link: "http://google.com", newWindow: false, edit:false },
                { caption: 'Umbraco', link: "http://umbraco.com", newWindow: false, edit: false },
                { caption: 'Nibble', link: "http://nibble.be", newWindow: false, edit: false }
            ];

            $scope.internal = function ($event) {
                var d = dialogService.contentPicker({ scope: $scope, multipicker: false, callback: select });

                $event.preventDefault();
            };

            $scope.edit = function (idx) {
                for (var i = 0; i < $scope.relatedLinks.length; i++) {
                    $scope.relatedLinks[i].edit = false;
                }
                $scope.relatedLinks[idx].edit = true;
            };

            $scope.cancelEdit = function(idx) {
                $scope.relatedLinks[idx].edit = false;
            };
            
            $scope.delete = function (idx) {
                
              $scope.relatedLinks.splice($scope.relatedLinks[idx], 1);
                
            };

            $scope.add = function () {

                if ($scope.addExternal) {
                    var newExtLink = new function() {
                        this.caption = $scope.newCaption;
                        this.link = $scope.newLink;
                        this.newWindow = $scope.newNewWindow;
                    };
                    $scope.relatedLinks.push(newExtLink);
                } else {
                    var newIntLink = new function () {
                        this.caption = $scope.newCaption;
                        this.link = $scope.newLink;
                        this.newWindow = $scope.newNewWindow;
                        this.internal = $scope.newInternal;
                    };
                    $scope.relatedLinks.push(newIntLink);
                }
                $scope.newCaption = '';
                $scope.newLink = 'http://';
                $scope.newNewWindow = false;
                $scope.newInternal = null;
                $scope.newInternalName = '';
                
               
            };

            $scope.switch = function ($event) {
                $scope.addExternal = !$scope.addExternal;
                $event.preventDefault();
            };
            
            function select(data) {
                $scope.newInternal = data.id;
                $scope.newInternalName = data.name;
            }

            

        });