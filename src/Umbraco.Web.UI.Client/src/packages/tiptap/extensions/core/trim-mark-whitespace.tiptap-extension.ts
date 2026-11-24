import { Extension, TextSelection, commands } from '../../externals.js';

/**
 * Keeps leading and trailing whitespace of a selection outside of any mark that is applied to it,
 * e.g. when a word is double-clicked on a platform that includes the trailing space in the selection.
 */
export const UmbTrimMarkWhitespace = Extension.create({
	name: 'umbTrimMarkWhitespace',

	addCommands() {
		return {
			setMark: (typeOrName, attributes) => (props) => {
				const { tr, state } = props;
				const { selection } = tr;

				if (selection instanceof TextSelection && !selection.empty) {
					const text = state.doc.textBetween(selection.from, selection.to, '\n', '\n');
					const leading = text.length - text.trimStart().length;
					const trailing = text.length - text.trimEnd().length;

					if (leading + trailing < text.length && (leading || trailing)) {
						tr.setSelection(TextSelection.create(tr.doc, selection.from + leading, selection.to - trailing));
					}
				}

				return commands.setMark(typeOrName, attributes)(props);
			},
		};
	},
});
