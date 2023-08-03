import type { ManifestPropertyAction } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestPropertyAction> = [
	{
		type: 'propertyAction',
		alias: 'Umb.PropertyAction.Copy',
		name: 'Copy Property Action',
		loader: () => import('./common/copy/property-action-copy.element.js'),
		meta: {
			propertyEditors: ['Umb.PropertyEditorUi.TextBox'],
		},
	},
	{
		type: 'propertyAction',
		alias: 'Umb.PropertyAction.Clear',
		name: 'Clear Property Action',
		loader: () => import('./common/clear/property-action-clear.element.js'),
		meta: {
			propertyEditors: ['Umb.PropertyEditorUi.TextBox'],
		},
	},
];
