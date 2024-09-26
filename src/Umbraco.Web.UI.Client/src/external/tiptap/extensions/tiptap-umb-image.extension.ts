import Image from '@tiptap/extension-image';

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
				default: '100%',
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
