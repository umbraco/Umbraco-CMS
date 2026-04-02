import { Extension } from '../../externals.js';
import type { Attributes } from '../../externals.js';

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		htmlIdAttribute: {
			setId: (id?: string, type?: string) => ReturnType;
			toggleId: (id?: string, type?: string) => ReturnType;
			unsetId: (type?: string) => ReturnType;
		};
	}
}

export interface UmbTiptapHtmlIdAttributeOptions {
	types: Array<string>;
}

export const HtmlIdAttribute = Extension.create<UmbTiptapHtmlIdAttributeOptions>({
	name: 'htmlIdAttribute',

	addOptions() {
		return { types: [] };
	},

	addGlobalAttributes() {
		return [
			{
				types: this.options.types,
				attributes: { id: {} } as Attributes,
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
