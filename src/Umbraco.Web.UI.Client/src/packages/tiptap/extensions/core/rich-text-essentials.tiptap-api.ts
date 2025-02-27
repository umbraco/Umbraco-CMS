import { UmbTiptapExtensionApiBase } from '../base.js';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import {
	Div,
	HtmlGlobalAttributes,
	Placeholder,
	Span,
	StarterKit,
	TrailingNode,
} from '@umbraco-cms/backoffice/external/tiptap';
import { css } from '@umbraco-cms/backoffice/external/lit';

export class UmbTiptapRichTextEssentialsExtensionApi extends UmbTiptapExtensionApiBase {
	#localize = new UmbLocalizationController(this);

	getTiptapExtensions = () => [
		StarterKit,
		Placeholder.configure({ placeholder: this.#localize.term('placeholders_rteParagraph') }),
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

	override getStyles = () => css`
		pre {
			background-color: var(--uui-color-surface-alt);
			padding: var(--uui-size-space-2) var(--uui-size-space-4);
			border-radius: calc(var(--uui-border-radius) * 2);
			overflow-x: auto;
		}

		code:not(pre > code) {
			background-color: var(--uui-color-surface-alt);
			padding: var(--uui-size-space-1) var(--uui-size-space-2);
			border-radius: calc(var(--uui-border-radius) * 2);
		}

		code {
			font-family: 'Roboto Mono', monospace;
			background: none;
			color: inherit;
			font-size: 0.8rem;
			padding: 0;
		}

		h1,
		h2,
		h3,
		h4,
		h5,
		h6 {
			margin-top: 0;
			margin-bottom: 0.5em;
		}

		li {
			> p {
				margin: 0;
				padding: 0;
			}
		}
	`;
}

export default UmbTiptapRichTextEssentialsExtensionApi;

export { UmbTiptapRichTextEssentialsExtensionApi as api };
