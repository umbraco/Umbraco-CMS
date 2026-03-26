import { Image, ProseMirrorPlugin } from '../../externals.js';
import { UMB_TIPTAP_NODE_DBLCLICK_EVENT } from '../tiptap-node-dblclick.event.js';

export interface UmbImageAttributes {
	src: string;
	alt?: string;
	title?: string;
	width?: string;
	height?: string;
	loading?: string;
	srcset?: string;
	sizes?: string;
	'data-tmpimg'?: string;
	'data-udi'?: string;
}

export const UmbImage = Image.extend({
	addAttributes() {
		return {
			...this.parent?.(),
			width: {
				default: null,
			},
			height: {
				default: null,
			},
			loading: {
				default: null,
			},
			srcset: {
				default: null,
			},
			sizes: {
				default: null,
			},
			'data-tmpimg': { default: null },
			'data-udi': { default: null },
		};
	},

	addProseMirrorPlugins() {
		const name = this.name;
		return [
			...(this.parent?.() ?? []),
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

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		umbImage: {
			/**
			 * Add an image
			 * @param options The image attributes
			 * @example
			 * editor
			 *   .commands
			 *   .setImage({ src: 'https://tiptap.dev/logo.png', alt: 'tiptap', title: 'tiptap logo' })
			 */
			setImage: (options: UmbImageAttributes) => ReturnType;
		};
	}
}
