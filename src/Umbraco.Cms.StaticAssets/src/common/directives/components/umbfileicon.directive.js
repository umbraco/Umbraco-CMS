/**
@ngdoc directive
@name umbraco.directives.directive:umbFileIcon
@restrict E
@scope

@description
Use this directive to render a file icon.

<h3>Markup example</h3>
<pre>
    <div>

        <umb-file-icon
            extension="pdf"
            size="s">
        </umb-file-icon>

	</div>
</pre>

@param {string} size (<code>attribute</code>): This parameter defines the size of the file icon (s, m).
**/

(function () {
    'use strict';

    function umbFileIconController() {

        var vm = this;

        if (!vm.icon) {
            vm.icon = 'icon-document';
        }
    }

    var component = {
        templateUrl: 'views/components/umb-file-icon.html',
        bindings: {
            extension: "@?",
            icon: "@?",
            size: "@?",
            text: "@?"
        },
        controllerAs: 'vm',
        controller: umbFileIconController
    };

    angular.module('umbraco.directives').component('umbFileIcon', component);

})();
