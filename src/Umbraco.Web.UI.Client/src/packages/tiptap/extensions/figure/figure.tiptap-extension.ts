import { mergeAttributes, Node, ProseMirrorPlugin } from '../../externals.js';
import { UMB_TIPTAP_NODE_DBLCLICK_EVENT } from '../tiptap-node-dblclick.event.js';

export interface UmbTiptapFigureOptions {
	/**
	 * HTML attributes to add to the image element.
	 * @default {}
	 * @example { class: 'foo' }
	 */
	HTMLAttributes: Record<string, any>;
}

export const Figure = Node.create<UmbTiptapFigureOptions>({
	name: 'figure',
	group: 'block',
	content: 'block+',
	draggable: true,
	selectable: true,
	isolating: true,
	atom: true,

	addAttributes() {
		return {
			figcaption: {
				default: '',
			},
		};
	},

	addOptions() {
		return {
			HTMLAttributes: {},
		};
	},

	parseHTML() {
		return [
			{
				tag: this.name,
				getAttrs: (dom) => {
					const figcaption = dom.querySelector('figcaption');
					return {
						figcaption: figcaption?.textContent || '',
					};
				},
			},
		];
	},

	renderHTML({ HTMLAttributes }) {
		return [this.name, mergeAttributes(this.options.HTMLAttributes, HTMLAttributes), 0];
	},

	addProseMirrorPlugins() {
		const name = this.name;
		return [
			new ProseMirrorPlugin({
				props: {
					handleDoubleClickOn: (view, _pos, node) => {
						if (node.type.name === name) {
							view.dom.dispatchEvent(new CustomEvent(UMB_TIPTAP_NODE_DBLCLICK_EVENT, { bubbles: true, composed: true }));
							return true;
						}
						return false;
					},
				},
			}),
		];
	},
});
