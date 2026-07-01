import type { ManifestUfmComponent } from '../extensions/ufm-component.extension.js';
import { manifests as elementNameManifests } from './element-name/manifests.js';

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
		meta: { alias: 'umbContentName' },
	},
	...elementNameManifests,
	{
		type: 'ufmComponent',
		alias: 'Umb.Markdown.Link',
		name: 'Link UFM Component',
		api: () => import('./link/link.component.js'),
		meta: { alias: 'umbLink' },
	},
	{
		type: 'ufmComponent',
		alias: 'Umb.Markdown.MemberName',
		name: 'Member Name UFM Component',
		api: () => import('./member-name/member-name.component.js'),
		meta: { alias: 'umbMemberName' },
	},
];
