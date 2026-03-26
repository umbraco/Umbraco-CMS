import { mergeAttributes, Node, ProseMirrorPlugin } from '../../externals.js';
import { UMB_TIPTAP_NODE_DBLCLICK_EVENT } from '../tiptap-node-dblclick.event.js';

export interface UmbEmbeddedMediaOptions {
	inline: boolean;
}

export const umbEmbeddedMedia = Node.create<UmbEmbeddedMediaOptions>({
	name: 'umbEmbeddedMedia',
	group() {
		return this.options.inline ? 'inline' : 'block';
	},
	inline() {
		return this.options.inline;
	},

	atom: true,
	marks: '',
	draggable: true,
	selectable: true,

	addAttributes() {
		return {
			'data-embed-constrain': { default: false },
			'data-embed-height': { default: 240 },
			'data-embed-url': { default: null },
			'data-embed-width': { default: 360 },
			markup: { default: null, parseHTML: (element) => element.innerHTML },
		};
	},

	parseHTML() {
		return [{ tag: '.umb-embed-holder', priority: 100 }];
	},

	renderHTML({ HTMLAttributes }) {
		const { markup, ...attrs } = HTMLAttributes;
		const embed = document.createRange().createContextualFragment(markup);
		return [this.options.inline ? 'span' : 'div', mergeAttributes({ class: 'umb-embed-holder' }, attrs), embed];
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

	addCommands() {
		return {
			setEmbeddedMedia:
				(options) =>
				({ commands }) => {
					const attrs = {
						markup: options.markup,
						'data-embed-url': options.url,
						'data-embed-width': options.width,
						'data-embed-height': options.height,
						'data-embed-constrain': options.constrain,
					};
					return commands.insertContent({ type: this.name, attrs });
				},
		};
	},
});

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		umbEmbeddedMedia: {
			setEmbeddedMedia: (options: {
				markup: string;
				url: string;
				width?: string;
				height?: string;
				constrain?: boolean;
			}) => ReturnType;
		};
	}
}
