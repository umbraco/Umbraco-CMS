import { Extension } from '../../externals.js';
import type { Attributes } from '../../externals.js';

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		htmlStyleAttribute: {
			setStyles: (style?: string, type?: string) => ReturnType;
			toggleStyles: (style?: string, type?: string) => ReturnType;
			unsetStyles: (type?: string) => ReturnType;
		};
	}
}

export interface UmbTiptapHtmlStyleAttributeOptions {
	types: Array<string>;
}

export const HtmlStyleAttribute = Extension.create<UmbTiptapHtmlStyleAttributeOptions>({
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
