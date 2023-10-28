/**
@ngdoc directive
@name umbraco.directives.directive:umbPagination
@restrict E
@scope

@description
Use this directive to generate a pagination.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-pagination
            page-number="vm.pagination.pageNumber"
            total-pages="vm.pagination.totalPages"
            on-next="vm.nextPage"
            on-prev="vm.prevPage"
            on-change="vm.changePage"
            on-go-to-page="vm.goToPage">
        </umb-pagination>

    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller() {

            var vm = this;

            vm.pagination = {
                pageNumber: 1,
                totalPages: 10
            };

            vm.nextPage = nextPage;
            vm.prevPage = prevPage;
            vm.changePage = changePage;
            vm.goToPage = goToPage;

            function nextPage(pageNumber) {
                // do magic here
                console.log(pageNumber);
                alert("nextpage");
            }

            function prevPage(pageNumber) {
                // do magic here
                console.log(pageNumber);
                alert("prevpage");
            }
            
            function changePage(pageNumber) {
                // do magic here
                console.log(pageNumber);
                alert("changepage");
            }

            function goToPage(pageNumber) {
                // do magic here
                console.log(pageNumber);
                alert("go to");
            }

        }

        angular.module("umbraco").controller("My.Controller", Controller);
    })();
</pre>

@param {number} pageNumber (<code>binding</code>): Current page number.
@param {number} totalPages (<code>binding</code>): The total number of pages.
@param {callback} onNext (<code>binding</code>): Callback method to go to the next page.
    <h3>The callback returns:</h3>
    <ul>
        <li><code>pageNumber</code>: The page number</li>
    </ul>
@param {callback=} onPrev (<code>binding</code>): Callback method to go to the previous page.
    <h3>The callback returns:</h3>
    <ul>
        <li><code>pageNumber</code>: The page number</li>
    </ul>
@param {callback=} onGoToPage (<code>binding</code>): Callback method to go to a specific page.
    <h3>The callback returns:</h3>
    <ul>
        <li><code>pageNumber</code>: The page number</li>
    </ul>
@param {callback=} onChange (<code>binding</code>): Callback method when changing page.
    <h3>The callback returns:</h3>
    <ul>
        <li><code>pageNumber</code>: The page number</li>
    </ul>
**/

(function() {
   'use strict';

   function PaginationDirective(localizationService) {

      function link(scope, el, attr, ctrl) {

         function activate() {
            // page number is sometimes a string - let's make sure it's an int before we do anything with it
            if (scope.pageNumber) {
                scope.pageNumber = parseInt(scope.pageNumber);
            }

            let tempPagination = [];
             
            var i = 0;

            if (scope.totalPages <= 10) {
                for (i = 0; i < scope.totalPages; i++) {
                    tempPagination.push({
                        val: (i + 1),
                        isActive: scope.pageNumber === (i + 1)
                    });
                }
            }
            else {
                //if there is more than 10 pages, we need to do some fancy bits

                //get the max index to start
                var maxIndex = scope.totalPages - 10;
                //set the start, but it can't be below zero
                var start = Math.max(scope.pageNumber - 5, 0);
                //ensure that it's not too far either
                start = Math.min(maxIndex, start);

                for (i = start; i < (10 + start) ; i++) {
                    tempPagination.push({
                        val: (i + 1),
                        isActive: scope.pageNumber === (i + 1)
                    });
                }

                //now, if the start is greater than 0 then '1' will not be displayed, so do the elipses thing
                if (start > 0) {
                    localizationService.localize("general_first").then(function(value){
                        var firstLabel = value;
                        tempPagination.unshift({ name: firstLabel, val: 1, isActive: false }, {val: "...",isActive: false});
                    });
                }

                //same for the end
                if (start < maxIndex) {
                    localizationService.localize("general_last").then(function(value){
                        var lastLabel = value;
                        tempPagination.push({ val: "...", isActive: false }, { name: lastLabel, val: scope.totalPages, isActive: false });
                    });
                }
            }

            scope.pagination = tempPagination;
         }

         scope.next = function () {
             if (scope.pageNumber < scope.totalPages) {
                 scope.pageNumber++;
                 if (scope.onNext) {
                     scope.onNext(scope.pageNumber);
                 }
                 if (scope.onChange) {
                     scope.onChange({ "pageNumber": scope.pageNumber });
                 }
             }
         };

         scope.prev = function (pageNumber) {
             if (scope.pageNumber > 1) {
                 scope.pageNumber--;
                 if (scope.onPrev) {
                     scope.onPrev(scope.pageNumber);
                 }
                 if (scope.onChange) {
                     scope.onChange({ "pageNumber": scope.pageNumber });
                 }
             }
         };

         scope.goToPage = function (pageNumber) {
             scope.pageNumber = pageNumber + 1;
             if (scope.onGoToPage) {
                 scope.onGoToPage(scope.pageNumber);
             }
             if (scope.onChange) {
                 scope.onChange({ "pageNumber": scope.pageNumber });
             }
         };

         var unbindPageNumberWatcher =  scope.$watchCollection('[pageNumber, totalPages]', function (newValues, oldValues) {
            activate();
         });

         scope.$on('$destroy', function(){
           unbindPageNumberWatcher();
         });

         activate();

      }

      var directive = {
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/umb-pagination.html',
         scope: {
            pageNumber: "=",
            totalPages: "=",
            onNext: "=",
            onPrev: "=",
            onGoToPage: "=",
            onChange: "&"
         },
         link: link
      };

      return directive;

   }

   angular.module('umbraco.directives').directive('umbPagination', PaginationDirective);

})();
