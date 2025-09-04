import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Extension } from '@umbraco-cms/backoffice/external/tiptap';
import type { Attributes } from '@umbraco-cms/backoffice/external/tiptap';

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		htmlDatasetAttributes: {
			setId: (id?: string, type?: string) => ReturnType;
			toggleId: (id?: string, type?: string) => ReturnType;
			unsetId: (type?: string) => ReturnType;
		};
	}
}

/**
 * Converts camelCase to kebab-case.
 * @param {string} str - The string to convert.
 * @returns {string} The converted string.
 */
function camelCaseToKebabCase(str: string): string {
	return str.replace(/[A-Z]+(?![a-z])|[A-Z]/g, ($, ofs) => (ofs ? '-' : '') + $.toLowerCase());
}

interface HtmlDatasetAttributesOptions {
	types: Array<string>;
}

const HtmlDatasetAttributes = Extension.create<HtmlDatasetAttributesOptions>({
	name: 'htmlDatasetAttributes',

	addOptions() {
		return { types: [] };
	},

	addGlobalAttributes() {
		return [
			{
				types: this.options.types,
				attributes: {
					dataset: {
						parseHTML: (element) => element.dataset,
						renderHTML: (attributes) => {
							const keys = attributes.dataset ? Object.keys(attributes.dataset) : [];
							if (!keys.length) return {};
							const dataAtrrs: Record<string, string> = {};
							keys.forEach((key) => {
								dataAtrrs['data-' + camelCaseToKebabCase(key)] = attributes.dataset[key];
							});
							return dataAtrrs;
						},
					},
				} as Attributes,
			},
		];
	},

	addCommands() {
		return {
			setId:
				(id, type) =>
				({ commands }) => {
					if (!id) return false;
					const types = type ? [type] : this.options.types;
					return types.map((type) => commands.updateAttributes(type, { id })).every((response) => response);
				},
			toggleId:
				(id, type) =>
				({ commands, editor }) => {
					if (!id) return false;
					const types = type ? [type] : this.options.types;
					const existing = types.map((type) => editor.getAttributes(type)?.id as string).filter((x) => x);
					return existing.length ? commands.unsetId(type) : commands.setId(id, type);
				},
			unsetId:
				(type) =>
				({ commands }) => {
					const types = type ? [type] : this.options.types;
					return types.map((type) => commands.resetAttributes(type, 'id')).every((response) => response);
				},
		};
	},
});

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
