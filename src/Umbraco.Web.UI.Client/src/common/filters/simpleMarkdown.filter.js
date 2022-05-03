/**
* @ngdoc filter
* @name umbraco.filters.simpleMarkdown
* @description 
* Used when rendering a string as Markdown as HTML (i.e. with ng-bind-html). Allows use of **bold**, *italics*, ![images](url) and [links](url)
**/
angular.module("umbraco.filters").filter('simpleMarkdown', function () {
  return function (text) {
	if (!text) {
		return '';
    }
    if (window.Markdown) {
      var converter = new window.Markdown.Converter();
      var markup = converter.makeHtml(text).trim();
      return markup.replace('<a', '<a target="_blank" rel="noopener" class="underline" ');
    }
    return text;
  };
});
