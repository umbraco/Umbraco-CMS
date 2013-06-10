/*! umbraco - v0.0.1-SNAPSHOT - 2013-06-10
 * http://umbraco.github.io/Belle
 * Copyright (c) 2013 Per Ploug, Anders Stenteberg & Shannon Deminick;
 * Licensed MIT
 */
'use strict';
define(['angular'], function (angular) {
var app = angular.module('umbraco', [
  	'umbraco.filters',
  	'umbraco.directives',
  	'umbraco.mocks.resources',
  	'umbraco.services'
]);

return app;
});