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

    return text
      .replace(/\*\*(.*)\*\*/gim, '<b>$1</b>')
      .replace(/\*(.*)\*/gim, '<i>$1</i>')
      .replace(/!\[(.*?)\]\((.*?)\)/gim, "<img alt='$1' src='$2' />")
      .replace(/\[(.*?)\]\((.*?)\)/gim, "<a href='$2' target='_blank' class='underline'>$1</a>")
      .replace(/\n/g, '<br />').trim();
  };
});
