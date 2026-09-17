import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.DocumentStartNodeAccess',
	name: 'Document Start Node Access Property Editor UI',
	element: () => import('./document-start-node-access-property-editor-ui.element.js'),
	meta: {
		label: 'Document Start Node Access',
		icon: 'icon-page',
		group: '#propertyEditorUIGroups_pickers',
		supportsReadOnly: true,
	},
};

export const manifests: Array<UmbExtensionManifest> = [manifest];
