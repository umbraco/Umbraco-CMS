import Image from '@tiptap/extension-image';

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
		};
	},
});
