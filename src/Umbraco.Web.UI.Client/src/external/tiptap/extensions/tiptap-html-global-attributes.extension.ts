import { Extension } from '@tiptap/core';
import type { Attributes } from '@tiptap/core';

/**
 * Converts camelCase to kebab-case.
 * @param {string} str - The string to convert.
 * @returns {string} The converted string.
 */
function camelCaseToKebabCase(str: string): string {
	return str.replace(/[A-Z]+(?![a-z])|[A-Z]/g, ($, ofs) => (ofs ? '-' : '') + $.toLowerCase());
}

export interface HtmlGlobalAttributesOptions {
	/**
	 * The types where the text align attribute can be applied.
	 * @default []
	 * @example ['heading', 'paragraph']
	 */
	types: Array<string>;
}

export const HtmlGlobalAttributes = Extension.create<HtmlGlobalAttributesOptions>({
	name: 'htmlGlobalAttributes',

	addOptions() {
		return { types: [] };
	},

	addGlobalAttributes() {
		return [
			{
				types: this.options.types,
				attributes: {
					class: {},
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
					id: {},
					style: {},
					// style: {
					// 	parseHTML: (element) => (element.style?.length ? element.style : null),
					// 	renderHTML: (attributes) => {
					// 		if (!attributes.style?.length) return null;
					// 		return { style: attributes.style.cssText };
					// 	},
					// },
				} as Attributes,
			},
		];
	},
});
