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
					style: {
						parseHTML: (element) => (element.style.length ? element.style.cssText : null),
					},
				} as Attributes,
			},
		];
	},

	addCommands() {
		return {
			setClassName:
				(className, type) =>
				({ commands }) => {
					if (!className) return false;
					const types = type ? [type] : this.options.types;
					return types
						.map((type) => commands.updateAttributes(type, { class: className }))
						.every((response) => response);
				},
			unsetClassName:
				(type) =>
				({ commands }) => {
					const types = type ? [type] : this.options.types;
					return types.map((type) => commands.resetAttributes(type, 'class')).every((response) => response);
				},
			setId:
				(id, type) =>
				({ commands }) => {
					if (!id) return false;
					const types = type ? [type] : this.options.types;
					return types.map((type) => commands.updateAttributes(type, { id })).every((response) => response);
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

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		htmlGlobalAttributes: {
			setClassName: (className?: string, type?: string) => ReturnType;
			unsetClassName: (type?: string) => ReturnType;
			setId: (id?: string, type?: string) => ReturnType;
			unsetId: (type?: string) => ReturnType;
		};
	}
}
