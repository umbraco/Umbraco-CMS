import type { ManifestUfmComponent } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestUfmComponent> = [
	{
		type: 'ufmComponent',
		alias: 'Umb.Markdown.LabelValue',
		name: 'Label Value Markdown Component',
		api: () => import('./ufm-components/label-value.component.js'),
		meta: {
			marker: '=',
		},
	},
	{
		type: 'ufmComponent',
		alias: 'Umb.Markdown.Localize',
		name: 'Localize Markdown Component',
		api: () => import('./ufm-components/localize.component.js'),
		meta: {
			marker: '#',
		},
	},
];
