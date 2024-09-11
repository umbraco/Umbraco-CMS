import type { ManifestUfmComponent } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestUfmComponent> = [
	{
		type: 'ufmComponent',
		alias: 'Umb.Markdown.LabelValue',
		name: 'Label Value UFM Component',
		api: () => import('./label-value/label-value.component.js'),
		meta: { marker: '=' },
	},
	{
		type: 'ufmComponent',
		alias: 'Umb.Markdown.Localize',
		name: 'Localize UFM Component',
		api: () => import('./localize/localize.component.js'),
		meta: { marker: '#' },
	},
	{
		type: 'ufmComponent',
		alias: 'Umb.Markdown.ContentName',
		name: 'Content Name UFM Component',
		api: () => import('./content-name/content-name.component.js'),
		meta: { marker: '~' },
	},
];
