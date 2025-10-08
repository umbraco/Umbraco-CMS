import { Node, mergeAttributes } from '@tiptap/core';

/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
export const Anchor = Node.create({
	name: 'anchor',

	atom: true,
	draggable: true,
	inline: true,
	group: 'inline',
	marks: '',
	selectable: true,

	addAttributes() {
		return {
			id: {},
		};
	},

	addNodeView() {
		return ({ HTMLAttributes }) => {
			const dom = document.createElement('span');
			dom.setAttribute('data-umb-anchor', '');
			dom.setAttribute('title', HTMLAttributes.id);

			const icon = document.createElement('uui-icon');
			icon.setAttribute('name', 'icon-anchor');

			dom.appendChild(icon);

			return { dom };
		};
	},

	addOptions() {
		return {
			HTMLAttributes: { id: '' },
		};
	},

	parseHTML() {
		return [{ tag: 'a[id]', getAttrs: (element) => (element.innerHTML === '' ? {} : false) }];
	},

	renderHTML({ HTMLAttributes }) {
		return ['a', mergeAttributes(this.options.HTMLAttributes, HTMLAttributes)];
	},
});
