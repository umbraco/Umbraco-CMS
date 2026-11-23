import { callOrReturn, Code, flattenExtensions } from '../../externals.js';
import { UmbLink } from '../link/link.tiptap-extension.js';

export const UmbCode = Code.extend({
	// Tiptap's `Code` excludes every other mark (`_`), which also blocks a link from wrapping it.
	// Exclude every mark except the link instead.
	excludes() {
		const extensions = this.editor?.options.extensions;
		if (!extensions) return callOrReturn(this.parent);

		return flattenExtensions(extensions)
			.filter((extension) => extension.type === 'mark' && extension.name !== UmbLink.name)
			.map((extension) => extension.name)
			.join(' ');
	},
});
