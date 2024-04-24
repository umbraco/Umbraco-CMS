import { manifests as defaultManifests } from './components/property-action/manifests.js';
import type {
	ManifestPropertyActions,
	ManifestTypes,
	UmbBackofficeManifestKind,
} from '@umbraco-cms/backoffice/extension-registry';

export const propertyActionManifests: Array<ManifestPropertyActions> = [
	{
		type: 'propertyAction',
		kind: 'default',
		alias: 'Umb.PropertyAction.Copy',
		name: 'Copy Property Action',
		api: () => import('./common/copy/property-action-copy.controller.js'),
		forPropertyEditorUis: ['Umb.PropertyEditorUi.TextBox'],
		meta: {
			icon: 'icon-paste-in',
			label: 'Copy',
		},
	},
	{
		type: 'propertyAction',
		kind: 'default',
		alias: 'Umb.PropertyAction.Clear',
		name: 'Clear Property Action',
		api: () => import('./common/clear/property-action-clear.controller.js'),
		forPropertyEditorUis: ['Umb.PropertyEditorUi.TextBox'],
		meta: {
			icon: 'icon-trash',
			label: 'Clear',
		},
	},
];

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	...defaultManifests,
	...propertyActionManifests,
];
