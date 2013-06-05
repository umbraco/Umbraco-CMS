/*! umbraco - v0.0.1-SNAPSHOT - 2013-06-04
 * http://umbraco.github.io/Belle
 * Copyright (c) 2013 Per Ploug, Anders Stenteberg & Shannon Deminick;
 * Licensed MIT
 */
'use strict';
define(['angular'], function (angular) {
var app = angular.module('umbraco', [
  	'umbraco.filters',
  	'umbraco.directives',
  	'umbraco.resources.content',
  	'umbraco.resources.contentType',
  	'umbraco.resources.macro',
  	'umbraco.resources.media',
  	'umbraco.resources.template',
  	'umbraco.resources.user',
    'umbraco.resources.localization',
    'umbraco.resources.tags',
    'umbraco.services.notifications',
    'umbraco.services.navigation',
  	'umbraco.services.tree',
  	'umbraco.services.dialog',
  	'umbraco.services.search'
]);



return app;
});