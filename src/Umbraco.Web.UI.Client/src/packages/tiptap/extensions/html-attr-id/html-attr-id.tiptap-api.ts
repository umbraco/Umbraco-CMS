import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { HtmlIdAttribute } from './html-attr-id.tiptap-extension.js';

export default class UmbTiptapHtmlAttributeIdExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		HtmlIdAttribute.configure({
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
