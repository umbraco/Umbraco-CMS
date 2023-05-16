import type { ManifestPropertyAction } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestPropertyAction> = [
	{
		type: 'propertyAction',
		alias: 'Umb.PropertyAction.Copy',
		name: 'Copy Property Action',
		loader: () => import('./copy/property-action-copy.element'),
		conditions: {
			propertyEditors: ['Umb.PropertyEditorUI.TextBox'],
		},
	},
	{
		type: 'propertyAction',
		alias: 'Umb.PropertyAction.Clear',
		name: 'Clear Property Action',
		loader: () => import('./clear/property-action-clear.element'),
		conditions: {
			propertyEditors: ['Umb.PropertyEditorUI.TextBox'],
		},
	},
];
