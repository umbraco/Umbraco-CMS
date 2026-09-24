import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.MediaStartNodeAccess',
	name: 'Media Start Node Access Property Editor UI',
	element: () => import('./media-start-node-access-property-editor-ui.element.js'),
	meta: {
		label: 'Media Start Node Access',
		icon: 'icon-picture',
		group: '#propertyEditorUIGroups_pickers',
		supportsReadOnly: true,
	},
};

export const manifests: Array<UmbExtensionManifest> = [manifest];
