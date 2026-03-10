import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { HtmlDatasetAttributes } from './html-attr-dataset.tiptap-extension.js';

export default class UmbTiptapHtmlAttributeDatasetExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		HtmlDatasetAttributes.configure({
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
