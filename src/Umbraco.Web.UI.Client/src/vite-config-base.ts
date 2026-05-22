import type { BuildOptions, UserConfig, LibraryOptions } from 'vite';

interface UmbViteDefaultConfigArgs {
	base?: string;
	dist: BuildOptions['outDir'];
	external?: string | string[] | RegExp | RegExp[];
	entry?: LibraryOptions['entry'];
	plugins?: UserConfig['plugins'];
	/**
	 * Coalesces non-entry Rollup chunks smaller than this size (in bytes) into
	 * other chunks. Entry chunks are never removed, so public subpaths remain
	 * stable. Default: 10_000 — coalesces sub-10 KB shared chunks, cutting the
	 * emitted `.js` file count by roughly 70 % across the typical workspace
	 * without changing any public subpath. Pass `0` to disable coalescing.
	 *
	 * See {@link https://rollupjs.org/configuration-options/#output-experimentalminchunksize}.
	 */
	minChunkSize?: number;
}

const DEFAULT_MIN_CHUNK_SIZE = 10_000;

export const getDefaultConfig = (args: UmbViteDefaultConfigArgs): UserConfig => {
	const minChunkSize = args.minChunkSize ?? DEFAULT_MIN_CHUNK_SIZE;
	return {
		build: {
			target: 'es2022',
			lib: {
				entry: args.entry || ['index.ts', 'manifests.ts', 'umbraco-package.ts'],
				formats: ['es'],
			},
			outDir: args.dist,
			emptyOutDir: true,
			sourcemap: true,
			rollupOptions: {
				external: args.external || [/^@umbraco-cms/],
				output: {
					experimentalMinChunkSize: minChunkSize,
				},
			},
		},
		plugins: args.plugins,
		base: args.base,
	};
};
