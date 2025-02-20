import type { Editor } from '@umbraco-cms/backoffice/external/tinymce';

/**
 * Setup sanitization for preventing injecting arbitrary JavaScript execution in attributes:
 * https://github.com/advisories/GHSA-w7jx-j77m-wp65
 * https://github.com/advisories/GHSA-5vm8-hhgr-jcjp
 * @param editor
 */
export const uriAttributeSanitizer = (editor: Editor) => {
	const uriAttributesToSanitize = ['src', 'href', 'data', 'background', 'action', 'formaction', 'poster', 'xlink:href'];

	const parseUri = (function () {
		// Encapsulated JS logic.
		const safeSvgDataUrlElements = ['img', 'video'];
		const scriptUriRegExp = /((java|vb)script|mhtml):/i;
		// eslint-disable-next-line no-control-regex
		const trimRegExp = /[\s\u0000-\u001F]+/g;

		const isInvalidUri = (uri: string, tagName: string) => {
			if (/^data:image\//i.test(uri)) {
				return safeSvgDataUrlElements.indexOf(tagName) !== -1 && /^data:image\/svg\+xml/i.test(uri);
			} else {
				return /^data:/i.test(uri);
			}
		};

		return function parseUri(uri: string, tagName: string) {
			uri = uri.replace(trimRegExp, '');
			uri = decodeURIComponent(uri);

			if (scriptUriRegExp.test(uri)) {
				return;
			}

			if (isInvalidUri(uri, tagName)) {
				return;
			}

			return uri;
		};
	})();

	editor.serializer.addAttributeFilter('uriAttributesToSanitize', function (nodes) {
		nodes.forEach(function (node) {
			if (!node.attributes) return;
			for (const attr of node.attributes) {
				const attrName = attr.name.toLowerCase();
				if (uriAttributesToSanitize.indexOf(attrName) !== -1) {
					attr.value = parseUri(attr.value, node.name) ?? '';
				}
			}
		});
	});
};
