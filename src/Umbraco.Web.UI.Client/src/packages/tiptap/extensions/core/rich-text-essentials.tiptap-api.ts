import { UmbTiptapExtensionApiBase } from '../base.js';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { Div, HtmlGlobalAttributes, Placeholder, Span, StarterKit } from '@umbraco-cms/backoffice/external/tiptap';

export class UmbTiptapRichTextEssentialsExtensionApi extends UmbTiptapExtensionApiBase {
	#localize = new UmbLocalizationController(this);

	getTiptapExtensions = () => [
		StarterKit,
		Placeholder.configure({
			placeholder: ({ node }) => {
				return this.#localize.term(
					node.type.name === 'heading' ? 'placeholders_rteHeading' : 'placeholders_rteParagraph',
				);
			},
		}),
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
	];
}

export default UmbTiptapRichTextEssentialsExtensionApi;

export { UmbTiptapRichTextEssentialsExtensionApi as api };
