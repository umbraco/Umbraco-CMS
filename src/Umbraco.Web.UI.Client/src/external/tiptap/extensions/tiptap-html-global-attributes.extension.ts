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

/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
export interface HtmlGlobalAttributesOptions {
	/**
	 * The types where the text align attribute can be applied.
	 * @default []
	 * @example ['heading', 'paragraph']
	 */
	types: Array<string>;
}

/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
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
			toggleClassName:
				(className, type) =>
				({ commands, editor }) => {
					if (!className) return false;
					const types = type ? [type] : this.options.types;
					const existing = types.map((type) => editor.getAttributes(type)?.class as string).filter((x) => x);
					return existing.length ? commands.unsetClassName(type) : commands.setClassName(className, type);
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

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		htmlGlobalAttributes: {
			setClassName: (className?: string, type?: string) => ReturnType;
			toggleClassName: (className?: string, type?: string) => ReturnType;
			unsetClassName: (type?: string) => ReturnType;
			setId: (id?: string, type?: string) => ReturnType;
			toggleId: (id?: string, type?: string) => ReturnType;
			unsetId: (type?: string) => ReturnType;
			setStyles: (style?: string, type?: string) => ReturnType;
			toggleStyles: (style?: string, type?: string) => ReturnType;
			unsetStyles: (type?: string) => ReturnType;
		};
	}
}
