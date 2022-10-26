import type { ManifestPropertyAction, ManifestWithLoader } from '@umbraco-cms/models';

export const manifests: Array<ManifestWithLoader<ManifestPropertyAction>> = [
	{
		type: 'propertyAction',
		alias: 'Umb.PropertyAction.Copy',
		name: 'Copy Property Action',
		loader: () => import('./copy/property-action-copy.element'),
		meta: {
			propertyEditors: ['Umb.PropertyEditorUI.TextBox'],
		},
	},
	{
		type: 'propertyAction',
		alias: 'Umb.PropertyAction.Clear',
		name: 'Clear Property Action',
		loader: () => import('./clear/property-action-clear.element'),
		meta: {
			propertyEditors: ['Umb.PropertyEditorUI.TextBox'],
		},
	},
];
