import { manifest as memberPickerSchemaManifest } from './Umbraco.MemberPicker.js';

export const manifests: Array<UmbExtensionManifest> = [
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
			supportsReadOnly: true,
		},
	},
	memberPickerSchemaManifest,
];
