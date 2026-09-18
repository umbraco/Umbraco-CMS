import { defineConfig } from 'vite';
import { rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/external/tiptap';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

export default defineConfig({
	...getDefaultConfig({
		dist,
		base: '/umbraco/backoffice/external/tiptap',
		// Chunk coalescing must be disabled here: the whole point of this package is that every
		// entry shares one copy of each ProseMirror module. Rollup's default 10 KB chunk-size
		// heuristic could otherwise duplicate a small shared chunk (e.g. prosemirror-gapcursor)
		// into more than one entry, reintroducing the multiple-copies bug this package exists to fix.
		minChunkSize: 0,
		entry: {
			index: './index.ts',
			'pm/changeset': './pm/changeset.ts',
			'pm/commands': './pm/commands.ts',
			'pm/dropcursor': './pm/dropcursor.ts',
			'pm/gapcursor': './pm/gapcursor.ts',
			'pm/history': './pm/history.ts',
			'pm/inputrules': './pm/inputrules.ts',
			'pm/keymap': './pm/keymap.ts',
			'pm/model': './pm/model.ts',
			'pm/schema-list': './pm/schema-list.ts',
			'pm/state': './pm/state.ts',
			'pm/tables': './pm/tables.ts',
			'pm/transform': './pm/transform.ts',
			'pm/view': './pm/view.ts',
		},
	}),
});
