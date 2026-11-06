import { callOrReturn, Code, flattenExtensions } from '../../externals.js';
import { UmbLink } from '../link/link.tiptap-extension.js';

export const UmbCode = Code.extend({
	// Tiptap's `Code` excludes every other mark (`_`), which also rules out wrapping inline code in a link.
	// Keep excluding the other marks, so formatting still stays out of code, but let a link wrap it.
	excludes() {
		const extensions = this.editor?.options.extensions;
		if (!extensions) return callOrReturn(this.parent) ?? '_';

		return flattenExtensions(extensions)
			.filter((extension) => extension.type === 'mark' && extension.name !== UmbLink.name)
			.map((extension) => extension.name)
			.join(' ');
	},
});
