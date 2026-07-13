/* eslint-disable local-rules/enforce-umbraco-external-imports */

/* This Source Code has been derived from Tiptiz.
 * https://github.com/tiptiz/editor/blob/main/packages/tiptiz-extension-indent/src/indent.ts
 * SPDX-License-Identifier: MIT
 * Copyright © 2024 Owen Kriz.
 * Modifications are licensed under the MIT License.
 */

import { Extension } from '../../externals.js';
import type { Dispatch } from '../../externals.js';

import { AllSelection, TextSelection } from '@tiptap/pm/state';
import type { EditorState, Transaction } from '@tiptap/pm/state';

export interface UmbTiptapTextIndentOptions {
	minLevel: number;
	maxLevel: number;
	types: Array<string>;
}

export const TextIndent = Extension.create<UmbTiptapTextIndentOptions>({
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
							// Support both padding-left (current) and text-indent (legacy) for parsing
							const paddingLeft = element.style.paddingLeft;
							const textIndent = element.style.textIndent;
							const value = paddingLeft || textIndent;
							return value ? Math.max(minLevel, Math.min(maxLevel, parseInt(value, 10))) : null;
						},
						renderHTML: (attributes) => {
							if (!attributes.indent) return {};
							return {
								style: `padding-left: ${attributes.indent}rem;`,
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

	addKeyboardShortcuts() {
		return {
			Tab: () => {
				if (this.editor.isActive('listItem')) {
					// Attempt to sink; if not possible, just prevent focus loss
					this.editor.commands.sinkListItem('listItem');
					return true;
				}
				return this.editor.commands.textIndent();
			},
			'Shift-Tab': () => {
				if (this.editor.isActive('listItem')) {
					// Attempt to lift; if not possible, just prevent focus loss
					this.editor.commands.liftListItem('listItem');
					return true;
				}
				return this.editor.commands.textOutdent();
			},
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
