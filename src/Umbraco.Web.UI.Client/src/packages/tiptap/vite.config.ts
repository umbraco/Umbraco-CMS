import { defineConfig } from 'vite';
import { rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/packages/tiptap';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

export default defineConfig({
	...getDefaultConfig({
		dist,
		// Share one copy of Tiptap core and ProseMirror with the backoffice's `external/tiptap`
		// package instead of bundling our own — see external/tiptap/vite.config.ts. Extensions
		// that depend on `@tiptap/pm` (e.g. Tiptap Pro's Tracked Changes / Comments) need runtime
		// identity (PluginKey, Decoration/DecorationSet) with the copy the RTE itself is running.
		external: [/^@umbraco-cms/, /^@tiptap\/core$/, /^@tiptap\/pm\//, /^prosemirror-/],
	}),
});
