import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.DocumentBlueprintStartNodeAccess',
	name: 'Document Blueprint Start Node Access Property Editor UI',
	element: () => import('./document-blueprint-start-node-access-property-editor-ui.element.js'),
	meta: {
		label: 'Document Blueprint Start Node Access',
		icon: 'icon-blueprint',
		group: '#propertyEditorUIGroups_pickers',
		supportsReadOnly: true,
	},
};

export const manifests: Array<UmbExtensionManifest> = [manifest];
