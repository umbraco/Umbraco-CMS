/* This Source Code has been derived from Tiptiz.
 * https://github.com/tiptiz/editor/blob/main/packages/tiptiz-extension-indent/src/indent.ts
 * SPDX-License-Identifier: MIT
 * Copyright © 2024 Owen Kriz.
 * Modifications are licensed under the MIT License.
 */

import type { Dispatch } from '@tiptap/core';
import type { EditorState, Transaction } from '@tiptap/pm/state';

import { Extension } from '@tiptap/core';
import { AllSelection, TextSelection } from '@tiptap/pm/state';

/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
export interface TextIndentOptions {
	minLevel: number;
	maxLevel: number;
	types: Array<string>;
}

/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
export const TextIndent = Extension.create<TextIndentOptions>({
	name: 'textIndent',

	addOptions() {
		return {
			minLevel: 0,
			maxLevel: 5,
			types: ['heading', 'paragraph', 'listItem', 'taskItem'],
		};
	},

	addGlobalAttributes() {
		return [
			{
				types: this.options.types,
				attributes: {
					indent: {
						default: null,
						parseHTML: (element) => {
							const minLevel = this.options.minLevel;
							const maxLevel = this.options.maxLevel;
							const indent = element.style.textIndent;
							return indent ? Math.max(minLevel, Math.min(maxLevel, parseInt(indent, 10))) : null;
						},
						renderHTML: (attributes) => {
							if (!attributes.indent) return {};
							return {
								style: `text-indent: ${attributes.indent}rem;`,
							};
						},
					},
				},
			},
		];
	},

	addCommands() {
		const updateNodeIndentMarkup = (tr: Transaction, pos: number, delta: number) => {
			const node = tr.doc.nodeAt(pos);
			if (!node) return tr;

			const minLevel = this.options.minLevel;
			const maxLevel = this.options.maxLevel;

			let level = (node.attrs.indent || 0) + delta;
			level = Math.max(minLevel, Math.min(maxLevel, parseInt(level, 10)));

			if (level === node.attrs.indent) return tr;

			return tr.setNodeMarkup(pos, node.type, { ...node.attrs, indent: level }, node.marks);
		};

		const updateIndentLevel = (tr: Transaction, delta: number) => {
			if (tr.selection instanceof TextSelection || tr.selection instanceof AllSelection) {
				const { from, to } = tr.selection;
				tr.doc.nodesBetween(from, to, (node, pos) => {
					if (this.options.types.includes(node.type.name)) {
						tr = updateNodeIndentMarkup(tr, pos, delta);
						return false;
					}
					return true;
				});
			}
			return tr;
		};

		type CommanderArgs = {
			tr: Transaction;
			state: EditorState;
			dispatch: Dispatch;
		};

		const commanderFactory = (direction: number) => () =>
			function chainHandler({ tr, state, dispatch }: CommanderArgs) {
				const { selection } = state;
				tr.setSelection(selection);
				tr = updateIndentLevel(tr, direction);
				if (tr.docChanged) {
					if (dispatch instanceof Function) dispatch(tr);
					return true;
				}
				return false;
			};

		return {
			textIndent: commanderFactory(1),
			textOutdent: commanderFactory(-1),
		};
	},
});

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		textIndent: {
			textIndent: () => ReturnType;
			textOutdent: () => ReturnType;
		};
	}
}
