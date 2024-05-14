import { manifest as memberPickerSchemaManifest } from './Umbraco.MemberPicker.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.MemberPicker',
		name: 'Member Picker Property Editor UI',
		element: () => import('./property-editor-ui-member-picker.element.js'),
		meta: {
			label: 'Member Picker',
			propertyEditorSchemaAlias: 'Umbraco.MemberPicker',
			icon: 'icon-user',
			group: 'people',
		},
	},
	memberPickerSchemaManifest,
];
