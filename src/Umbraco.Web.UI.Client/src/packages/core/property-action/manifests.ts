import { manifests as defaultManifests } from './components/property-action/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	...defaultManifests,
	{
		type: 'propertyAction',
		kind: 'default',
		alias: 'Umb.PropertyAction.Copy',
		name: 'Copy Property Action',
		api: () => import('./common/copy/property-action-copy.controller.js'),
		forPropertyEditorUis: ['Umb.PropertyEditorUi.TextBox'],
	},
	{
		type: 'propertyAction',
		kind: 'default',
		alias: 'Umb.PropertyAction.Clear',
		name: 'Clear Property Action',
		api: () => import('./common/clear/property-action-clear.controller.js'),
		forPropertyEditorUis: ['Umb.PropertyEditorUi.TextBox'],
	},
];
