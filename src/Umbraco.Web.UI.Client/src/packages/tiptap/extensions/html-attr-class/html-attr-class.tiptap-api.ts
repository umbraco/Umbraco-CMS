import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { HtmlClassAttribute } from './html-attr-class.tiptap-extension.js';

export default class UmbTiptapHtmlAttributeClassExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		HtmlClassAttribute.configure({
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
