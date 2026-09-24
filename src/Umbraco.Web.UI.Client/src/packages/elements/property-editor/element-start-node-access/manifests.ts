import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ElementStartNodeAccess',
	name: 'Element Start Node Access Property Editor UI',
	element: () => import('./element-start-node-access-property-editor-ui.element.js'),
	meta: {
		label: 'Element Start Node Access',
		icon: 'icon-plugin',
		group: 'pickers',
		supportsReadOnly: true,
	},
};

export const manifests: Array<UmbExtensionManifest> = [manifest];
