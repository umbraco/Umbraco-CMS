import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.UserGroupPicker',
	name: 'User Group Picker Property Editor UI',
	element: () => import('./property-editor-ui-user-group-picker.element.js'),
	meta: {
		label: 'User Group Picker',
		icon: 'icon-users',
		group: '#propertyEditorUIGroups_people',
	},
};

export const manifests: Array<UmbExtensionManifest> = [manifest];
