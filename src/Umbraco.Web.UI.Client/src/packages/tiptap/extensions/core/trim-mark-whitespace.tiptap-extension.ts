import { Extension, Plugin, TextSelection, commands, isMarkActive } from '../../externals.js';
import type { CommandProps, Selection } from '../../externals.js';

const getTrimmedSelection = (selection: Selection) => {
	if (!(selection instanceof TextSelection) || selection.empty) return null;

	const { $from, $to, from, to } = selection;
	const start = $from.nodeAfter;
	const end = $to.nodeBefore;
	const leading = start?.isText ? /^\s*/.exec(start.text!)![0].length : 0;
	const trailing = end?.isText ? /\s*$/.exec(end.text!)![0].length : 0;

	return from + leading < to - trailing ? TextSelection.create($from.doc, from + leading, to - trailing) : null;
};

const runOnTrimmedSelection = (props: CommandProps, run: () => boolean) => {
	const { tr } = props;
	const original = tr.selection;
	const trimmed = getTrimmedSelection(original);

	if (!trimmed) return run();

	tr.setSelection(trimmed);
	// The command `state` caches its selection until `tr` is read, so read it to have `run` see the trimmed one.
	void props.state.tr;
	const result = run();
	tr.setSelection(TextSelection.create(tr.doc, original.anchor, original.head));
	return result;
};

/**
 * Keeps leading and trailing whitespace of a selection outside of any mark that is applied to it,
 * e.g. when a word is double-clicked on a platform that includes the trailing space in the selection.
 *
 * Commands are merged by name, so any further `setMark` or `toggleMark` behaviour belongs in these overrides
 * rather than in a second one.
 */
export const UmbTrimMarkWhitespace = Extension.create({
	name: 'umbTrimMarkWhitespace',

	addCommands() {
		return {
			setMark: (typeOrName, attributes) => (props) =>
				runOnTrimmedSelection(props, () => commands.setMark(typeOrName, attributes)(props)),

			toggleMark: (typeOrName, attributes, options) => (props) => {
				const toggle = () => commands.toggleMark(typeOrName, attributes, options)(props);
				return isMarkActive(props.state, typeOrName, attributes) ? toggle() : runOnTrimmedSelection(props, toggle);
			},
		};
	},

	// TODO: [LK] Review when `prosemirror-view` is upgraded to 1.43.0 or later, where the library handles mouse selection
	// itself and a double-click selects only the word. This plugin should then be redundant and can be removed.
	addProseMirrorPlugins() {
		return [
			new Plugin({
				props: {
					handleDOMEvents: {
						dblclick: (view) => {
							setTimeout(() => {
								const trimmed = getTrimmedSelection(view.state.selection);
								if (trimmed) view.dispatch(view.state.tr.setSelection(trimmed));
							});
							return false;
						},
					},
				},
			}),
		];
	},
});
