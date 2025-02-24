import { UmbTiptapExtensionApiBase } from '../base.js';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import {
	Div,
	HtmlGlobalAttributes,
	Placeholder,
	Span,
	StarterKit,
	TextStyle,
} from '@umbraco-cms/backoffice/external/tiptap';

export class UmbTiptapRichTextEssentialsExtensionApi extends UmbTiptapExtensionApiBase {
	#localize = new UmbLocalizationController(this);

	getTiptapExtensions = () => [
		StarterKit,
		Placeholder.configure({ placeholder: this.#localize.term('placeholders_rteParagraph') }),
		TextStyle,
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
				'textStyle',
				'underline',
				'umbLink',
			],
		}),
		Div,
		Span,
	];
}

export default UmbTiptapRichTextEssentialsExtensionApi;

export { UmbTiptapRichTextEssentialsExtensionApi as api };
