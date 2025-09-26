import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { HtmlStyleAttribute } from './html-attr-style.tiptap-extension.js';

export default class UmbTiptapHtmlAttributeStyleExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		HtmlStyleAttribute.configure({
			types: [
				'bold',
				'blockquote',
				'bulletList',
				'codeBlock',
				'div',
				'figcaption',
				'figure',
				'heading',
				'horizontalRule',
				'italic',
				'image',
				'link',
				'orderedList',
				'paragraph',
				'span',
				'strike',
				'subscript',
				'superscript',
				'table',
				'tableHeader',
				'tableRow',
				'tableCell',
				'underline',
				'umbLink',
			],
		}),
	];
}
