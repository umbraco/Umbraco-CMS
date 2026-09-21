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
	/**
	 * Coalesces the package's own lazily imported modules into a single chunk, so entering a feature
	 * costs one request rather than one per dynamic import. Rollup emits a chunk per dynamic import
	 * by default, which is cheap on a fast connection and expensive on a slow one.
	 *
	 * `eagerModules` names the modules that must stay out of that chunk: anything the application
	 * instantiates at startup rather than on demand. They are emitted as their own chunk, because
	 * leaving them for Rollup to place lets it hoist them into the lazy chunk and have the entry
	 * import that statically, which loads the whole chunk at boot. Put a module in the lazy chunk
	 * by mistake and it loads at boot too, so a package opts in only once it knows which of its
	 * modules are eager.
	 */
	lazyChunk?: {
		/** Chunk name. Defaults to `lazy`. */
		name?: string;
		/** Module id fragments or patterns to emit in the eager chunk instead. */
		eagerModules?: Array<string | RegExp>;
	};
}

const DEFAULT_MIN_CHUNK_SIZE = 10_000;

const isEager = (id: string, patterns: Array<string | RegExp>) =>
	patterns.some((pattern) => (typeof pattern === 'string' ? id.includes(pattern) : pattern.test(id)));

export const getDefaultConfig = (args: UmbViteDefaultConfigArgs): UserConfig => {
	const minChunkSize = args.minChunkSize ?? DEFAULT_MIN_CHUNK_SIZE;
	const lazyChunk = args.lazyChunk;
	// Rollup reports absolute ids, and only this package's own modules should be coalesced.
	const packageRoot = process.cwd();
	return {
		build: {
			target: 'es2022',
			lib: {
				entry: args.entry || ['index.ts', 'umbraco-package.ts'],
				formats: ['es'],
			},
			outDir: args.dist,
			emptyOutDir: true,
			sourcemap: true,
			rollupOptions: {
				external: args.external || [/^@umbraco-cms/],
				output: {
					experimentalMinChunkSize: minChunkSize,
					...(lazyChunk
						? {
								manualChunks(id: string) {
									if (!id.startsWith(packageRoot)) return;
									const name = lazyChunk.name ?? 'lazy';
									if (lazyChunk.eagerModules && isEager(id, lazyChunk.eagerModules)) {
										return `${name}-eager`;
									}
									return name;
								},
							}
						: {}),
				},
			},
		},
		plugins: args.plugins,
		base: args.base,
	};
};
