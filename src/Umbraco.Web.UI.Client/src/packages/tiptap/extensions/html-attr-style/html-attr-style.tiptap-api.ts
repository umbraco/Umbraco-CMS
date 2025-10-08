import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Extension } from '@umbraco-cms/backoffice/external/tiptap';
import type { Attributes } from '@umbraco-cms/backoffice/external/tiptap';

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		htmlStyleAttribute: {
			setStyles: (style?: string, type?: string) => ReturnType;
			toggleStyles: (style?: string, type?: string) => ReturnType;
			unsetStyles: (type?: string) => ReturnType;
		};
	}
}

interface HtmlStyleAttributeOptions {
	types: Array<string>;
}

const HtmlStyleAttribute = Extension.create<HtmlStyleAttributeOptions>({
	name: 'htmlStyleAttribute',

	addOptions() {
		return { types: [] };
	},

	addGlobalAttributes() {
		return [
			{
				types: this.options.types,
				attributes: {
					style: {
						parseHTML: (element) => (element.style.length ? element.style.cssText : null),
					},
				} as Attributes,
			},
		];
	},

	addCommands() {
		return {
			setStyles:
				(style, type) =>
				({ commands }) => {
					if (!style) return false;
					const types = type ? [type] : this.options.types;
					return types.map((type) => commands.updateAttributes(type, { style })).every((response) => response);
				},
			toggleStyles:
				(style, type) =>
				({ commands, editor }) => {
					if (!style) return false;
					const types = type ? [type] : this.options.types;
					const existing = types.map((type) => editor.getAttributes(type)?.style as string).filter((x) => x);
					return existing.length ? commands.unsetStyles(type) : commands.setStyles(style, type);
				},
			unsetStyles:
				(type) =>
				({ commands }) => {
					const types = type ? [type] : this.options.types;
					return types.map((type) => commands.resetAttributes(type, 'style')).every((response) => response);
				},
		};
	},
});

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
