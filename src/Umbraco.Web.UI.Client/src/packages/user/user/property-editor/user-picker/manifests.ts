import { manifest as userPickerSchemaManifest } from './Umbraco.UserPicker.js';
import { manifests as valueSummaryManifests } from './value-summary/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.UserPicker',
		name: 'User Picker Property Editor UI',
		element: () => import('./property-editor-ui-user-picker.element.js'),
		meta: {
			label: 'User Picker',
			propertyEditorSchemaAlias: 'Umbraco.UserPicker',
			icon: 'icon-user',
			group: '#propertyEditorUIGroups_people',
			keywords: ['select', 'user', 'author', 'owner', 'assignee', 'editor', 'creator', 'contributor', 'staff'],
		},
	},
	userPickerSchemaManifest,
	...valueSummaryManifests,
];
