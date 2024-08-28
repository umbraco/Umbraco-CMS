import type { ManifestUfmComponent } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestUfmComponent> = [
	{
		type: 'ufmComponent',
		alias: 'Umb.CustomUfmComponent',
		name: 'Custom UFM Component',
		api: () => import('./custom-ufm-component.js'),
		meta: {
			marker: '%',
		},
	},
];
