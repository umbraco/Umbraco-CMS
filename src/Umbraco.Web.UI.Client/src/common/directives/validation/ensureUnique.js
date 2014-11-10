/**
 * Validate that input items in a ng-repeate have a unique value. 
 * Eg: <div ui-sortable ng-model="myValueList">
 *  <div class="control-group" ng-repeat="item in myValueList">
 *      <input type="text" name="key" ng-model="item.key" placeholder="key" ng-required="true" ensure-unique="myValueList" />
 *      <span class="help-inline" val-msg-for="key" val-toggle-msg="unique">Duplicate</span>
 *    </ng-form>
 *  </div>
 *
 * This is sued for example in the keyValue list to ensure that each key is unique.
 */

angular.module('umbraco.directives').directive('ensureUnique', function () {
    return {
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {
            element.bind('blur', function (e, f) {
                if (!ngModel || !element.val())
                    ngModel.$setValidity('unique', true);
                else {

                    var currentValue = element.val();
                    var childAttr = attrs.ngModel.split(/\./).pop();
                    if (_.filter(scope.$parent.$eval(attrs.ensureUnique),
                        function (item) {
                            return item[childAttr] == currentValue;
                    }).length > 1)
                        ngModel.$setValidity('unique', false);
                    else
                        ngModel.$setValidity('unique', true);
                }

                // Update sibbling values affected by this change.
                if (e.originalEvent && e.originalEvent.type == 'blur')
                    $(this).parents("[ng-repeat]:first").siblings().find("[ng-model='" + attrs.ngModel + "']").blur();

            });
        }
    }
});