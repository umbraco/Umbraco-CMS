import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Extension } from '@umbraco-cms/backoffice/external/tiptap';
import type { Attributes } from '@umbraco-cms/backoffice/external/tiptap';

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		htmlIdAttribute: {
			setId: (id?: string, type?: string) => ReturnType;
			toggleId: (id?: string, type?: string) => ReturnType;
			unsetId: (type?: string) => ReturnType;
		};
	}
}

interface HtmlIdAttributeOptions {
	types: Array<string>;
}

const HtmlIdAttribute = Extension.create<HtmlIdAttributeOptions>({
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

export default class UmbTiptapHtmlAttributeIdExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		HtmlIdAttribute.configure({
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
