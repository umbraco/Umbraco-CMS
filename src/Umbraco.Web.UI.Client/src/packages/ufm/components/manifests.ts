import type { ManifestUfmComponent } from '../ufm-component.extension.js';

export const manifests: Array<ManifestUfmComponent> = [
	{
		type: 'ufmComponent',
		alias: 'Umb.Markdown.LabelValue',
		name: 'Label Value UFM Component',
		api: () => import('./label-value/label-value.component.js'),
		meta: { alias: 'umbValue', marker: '=' },
	},
	{
		type: 'ufmComponent',
		alias: 'Umb.Markdown.Localize',
		name: 'Localize UFM Component',
		api: () => import('./localize/localize.component.js'),
		meta: { alias: 'umbLocalize', marker: '#' },
	},
	{
		type: 'ufmComponent',
		alias: 'Umb.Markdown.ContentName',
		name: 'Content Name UFM Component',
		api: () => import('./content-name/content-name.component.js'),
		meta: { alias: 'umbContentName', marker: '~' },
	},
	{
		type: 'ufmComponent',
		alias: 'Umb.Markdown.Link',
		name: 'Link UFM Component',
		api: () => import('./link/link.component.js'),
		meta: { alias: 'umbLink' },
	},
];
