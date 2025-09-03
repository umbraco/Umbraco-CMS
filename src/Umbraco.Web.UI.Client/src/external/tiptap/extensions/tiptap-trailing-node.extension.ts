/* This Source Code has been derived from Tiptap.
 * https://github.com/ueberdosis/tiptap/blob/v2.11.5/demos/src/Experiments/TrailingNode/Vue/trailing-node.ts
 * SPDX-License-Identifier: MIT
 * Copyright © 2023 Tiptap GmbH.
 * Modifications are licensed under the MIT License.
 */

import { Extension } from '@tiptap/core';
import { Plugin, PluginKey } from '@tiptap/pm/state';

// @ts-ignore
function nodeEqualsType({ types, node }) {
	return (Array.isArray(types) && types.includes(node.type)) || node.type === types;
}

/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
export interface TrailingNodeOptions {
	node: string;
	notAfter: string[];
}

/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
export const TrailingNode = Extension.create<TrailingNodeOptions>({
	name: 'trailingNode',

	addOptions() {
		return {
			node: 'paragraph',
			notAfter: ['paragraph'],
		};
	},

	addProseMirrorPlugins() {
		const plugin = new PluginKey(this.name);
		const disabledNodes = Object.entries(this.editor.schema.nodes)
			.map(([, value]) => value)
			.filter((node) => this.options.notAfter.includes(node.name));

		return [
			new Plugin({
				key: plugin,
				appendTransaction: (_, __, state) => {
					const { doc, tr, schema } = state;
					const shouldInsertNodeAtEnd = plugin.getState(state);
					const endPosition = doc.content.size;
					const type = schema.nodes[this.options.node];

					if (!shouldInsertNodeAtEnd) {
						return;
					}

					return tr.insert(endPosition, type.create());
				},
				state: {
					init: (_, state) => {
						const lastNode = state.tr.doc.lastChild;

						return !nodeEqualsType({ node: lastNode, types: disabledNodes });
					},
					apply: (tr, value) => {
						if (!tr.docChanged) {
							return value;
						}

						const lastNode = tr.doc.lastChild;

						return !nodeEqualsType({ node: lastNode, types: disabledNodes });
					},
				},
			}),
		];
	},
});
