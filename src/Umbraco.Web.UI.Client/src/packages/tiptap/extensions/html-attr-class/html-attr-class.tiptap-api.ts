import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Extension } from '@umbraco-cms/backoffice/external/tiptap';
import type { Attributes } from '@umbraco-cms/backoffice/external/tiptap';

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		htmlClassAttribute: {
			setClassName: (className?: string, type?: string) => ReturnType;
			toggleClassName: (className?: string, type?: string) => ReturnType;
			unsetClassName: (type?: string) => ReturnType;
		};
	}
}

interface HtmlClassAttributeOptions {
	types: Array<string>;
}

const HtmlClassAttribute = Extension.create<HtmlClassAttributeOptions>({
	name: 'htmlClassAttribute',

	addOptions() {
		return { types: [] };
	},

	addGlobalAttributes() {
		return [
			{
				types: this.options.types,
				attributes: { class: {} } as Attributes,
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
		};
	},
});

export default class UmbTiptapHtmlAttributeClassExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		HtmlClassAttribute.configure({
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
