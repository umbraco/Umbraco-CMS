import type { BuildOptions, UserConfig, LibraryOptions } from 'vite';

interface UmbViteDefaultConfigArgs {
	dist: BuildOptions['outDir'];
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
			sourcemap: true,
			rollupOptions: {
				external: [/^@umbraco/],
			},
		},
		plugins: args.plugins,
	};
};
