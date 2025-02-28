import { UmbTiptapExtensionApiBase } from '../base.js';
import { Div, HtmlGlobalAttributes, Span, StarterKit, TrailingNode } from '@umbraco-cms/backoffice/external/tiptap';

export class UmbTiptapRichTextEssentialsExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		StarterKit,
		Div,
		Span,
		HtmlGlobalAttributes.configure({
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
		TrailingNode,
	];
}

export default UmbTiptapRichTextEssentialsExtensionApi;

export { UmbTiptapRichTextEssentialsExtensionApi as api };
