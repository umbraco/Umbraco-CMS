import type { ManifestMarkedExtension } from './marked-extension.extension.js';

export const manifests: Array<ManifestMarkedExtension> = [
	{
		type: 'markedExtension',
		alias: 'Umb.MarkedExtension.Ufm',
		name: 'UFM Marked Extension',
		api: () => import('./ufm-marked-extension.api.js'),
		meta: {
			alias: 'ufm',
		},
	},
	{
		type: 'markedExtension',
		alias: 'Umb.MarkedExtension.Ufmjs',
		name: 'UFM JS Marked Extension',
		api: () => import('./ufmjs-marked-extension.api.js'),
		meta: {
			alias: 'ufmjs',
		},
	},
];
