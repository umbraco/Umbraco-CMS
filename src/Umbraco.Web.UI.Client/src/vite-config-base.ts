import type { BuildOptions, UserConfig, LibraryOptions } from 'vite';

interface UmbViteDefaultConfigArgs {
	base?: string;
	dist: BuildOptions['outDir'];
	external?: string | string[] | RegExp | RegExp[];
	entry?: LibraryOptions['entry'];
	plugins?: UserConfig['plugins'];
}

export const getDefaultConfig = (args: UmbViteDefaultConfigArgs): UserConfig => {
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
			},
		},
		plugins: args.plugins,
		base: args.base,
	};
};
