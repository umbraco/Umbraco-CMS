import { manifest as memberGroupSchemaManifest } from './Umbraco.MemberGroupPicker.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.MemberGroupPicker',
		name: 'Member Group Picker Property Editor UI',
		element: () => import('./property-editor-ui-member-group-picker.element.js'),
		meta: {
			label: 'Member Group Picker',
			propertyEditorSchemaAlias: 'Umbraco.MemberGroupPicker',
			icon: 'icon-users-alt',
			group: 'people',
			supportsReadOnly: true,
		},
	},
	memberGroupSchemaManifest,
];
