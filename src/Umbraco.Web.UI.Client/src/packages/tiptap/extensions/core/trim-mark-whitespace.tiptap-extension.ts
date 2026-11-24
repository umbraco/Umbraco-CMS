import { Extension, TextSelection, commands } from '../../externals.js';

const getEdgeWhitespace = ({ $from, $to }: TextSelection) => {
	const start = $from.nodeAfter;
	const end = $to.nodeBefore;
	return {
		leading: start?.isText ? /^\s*/.exec(start.text!)![0].length : 0,
		trailing: end?.isText ? /\s*$/.exec(end.text!)![0].length : 0,
	};
};

/**
 * Keeps leading and trailing whitespace of a selection outside of any mark that is applied to it,
 * e.g. when a word is double-clicked on a platform that includes the trailing space in the selection.
 *
 * Commands are merged by name, so any further `setMark` behaviour belongs in this override rather than a second one.
 */
export const UmbTrimMarkWhitespace = Extension.create({
	name: 'umbTrimMarkWhitespace',

	addCommands() {
		return {
			setMark: (typeOrName, attributes) => (props) => {
				const { tr } = props;
				const { selection } = tr;

				if (selection instanceof TextSelection && !selection.empty) {
					const { from, to } = selection;
					const { leading, trailing } = getEdgeWhitespace(selection);

					if (from + leading < to - trailing) {
						tr.setSelection(TextSelection.create(tr.doc, from + leading, to - trailing));
						const result = commands.setMark(typeOrName, attributes)(props);
						tr.setSelection(TextSelection.create(tr.doc, from, to));
						return result;
					}
				}

				return commands.setMark(typeOrName, attributes)(props);
			},
		};
	},
});
