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
];
