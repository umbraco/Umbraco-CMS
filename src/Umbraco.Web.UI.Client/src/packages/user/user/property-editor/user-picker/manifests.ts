import { manifest as userPickerSchemaManifest } from './Umbraco.UserPicker.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.UserPicker',
		name: 'User Picker Property Editor UI',
		element: () => import('./property-editor-ui-user-picker.element.js'),
		meta: {
			label: 'User Picker',
			propertyEditorSchemaAlias: 'Umbraco.UserPicker',
			icon: 'icon-user',
			group: 'people',
		},
	},
	userPickerSchemaManifest,
];
