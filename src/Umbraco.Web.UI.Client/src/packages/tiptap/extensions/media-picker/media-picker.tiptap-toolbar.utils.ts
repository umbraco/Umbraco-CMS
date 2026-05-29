import type { Editor, ProseMirrorNode } from '../../externals.js';
import { NodeSelection } from '../../externals.js';

export type UmbTiptapMarkInput = { type: string; attrs?: Record<string, unknown> };

export type UmbFigureImageData = {
	imageAttrs: Record<string, unknown>;
	caption?: string;
	pos: number;
	marks: Array<UmbTiptapMarkInput>;
};

/**
 * Reads the marks (e.g. `umbLink`) off the selected image node. When the
 * selection is on a container that wraps an image (e.g. a `figure`, which is
 * atomic and can't be selected through), drills in to find the inner image's
 * marks. Returns an empty array when no image is reachable.
 * @param {unknown} selection The current editor selection.
 * @returns {Array<UmbTiptapMarkInput>} The marks on the image, or an empty array.
 */
export function extractImageMarks(selection: unknown): Array<UmbTiptapMarkInput> {
	if (!(selection instanceof NodeSelection)) return [];

	if (selection.node.type.name === 'image') {
		return selection.node.marks.map((mark) => ({ type: mark.type.name, attrs: { ...mark.attrs } }));
	}

	let marks: Array<UmbTiptapMarkInput> = [];
	selection.node.descendants((child) => {
		if (child.type.name === 'image') {
			marks = child.marks.map((mark) => ({ type: mark.type.name, attrs: { ...mark.attrs } }));
			return false;
		}
		return true;
	});
	return marks;
}

/**
 * Locates the `figure` that surrounds the current selection. Handles both a
 * `NodeSelection` directly on the figure (the common case for atomic figures)
 * and a selection inside the figure's descendants.
 * @param {Editor} editor The Tiptap editor instance.
 * @returns {{ node: ProseMirrorNode; pos: number } | undefined} The figure node and its document position, or `undefined` if no figure surrounds the selection.
 */
function findEnclosingFigure(editor: Editor): { node: ProseMirrorNode; pos: number } | undefined {
	const { selection } = editor.state;
	if (selection instanceof NodeSelection && selection.node.type.name === 'figure') {
		return { node: selection.node, pos: selection.from };
	}
	const { $from } = selection;
	for (let depth = $from.depth; depth >= 0; depth--) {
		const node = $from.node(depth);
		if (node.type.name === 'figure') {
			return { node, pos: $from.before(depth) };
		}
	}
	return undefined;
}

/**
 * Reads the wrapping `figure` node's attributes from the current selection, so
 * they can be re-applied when the figure is rebuilt.
 * @param {Editor} editor The Tiptap editor instance.
 * @returns {Record<string, unknown> | undefined} The figure's attrs, or `undefined` if no figure surrounds the selection.
 */
export function extractFigureAttrs(editor: Editor): Record<string, unknown> | undefined {
	const figure = findEnclosingFigure(editor);
	return figure ? { ...figure.node.attrs } : undefined;
}

/**
 * Locates the `figure` that surrounds the current selection and pulls out the
 * inner image's attributes and marks, the figcaption text, and the figure's
 * document position so callers can replace it.
 * @param {Editor} editor The Tiptap editor instance.
 * @returns {UmbFigureImageData | undefined} The figure data, or `undefined` if no figure surrounds the selection or it contains no image with a `data-udi`.
 */
export function extractFigureImageData(editor: Editor): UmbFigureImageData | undefined {
	const figure = findEnclosingFigure(editor);
	if (!figure) return undefined;

	let imageAttrs: Record<string, unknown> = {};
	let caption: string | undefined;
	let marks: Array<UmbTiptapMarkInput> = [];

	figure.node.descendants((child) => {
		if (child.type.name === 'image') {
			imageAttrs = { ...child.attrs };
			marks = child.marks.map((mark) => ({ type: mark.type.name, attrs: { ...mark.attrs } }));
			return false;
		}
		if (child.type.name === 'figcaption') {
			caption = child.textContent || undefined;
			return false;
		}
		return true;
	});

	if (!imageAttrs['data-udi']) return undefined;

	return { imageAttrs, caption, pos: figure.pos, marks };
}
