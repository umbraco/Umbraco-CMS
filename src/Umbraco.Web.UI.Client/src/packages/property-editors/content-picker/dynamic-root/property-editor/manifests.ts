import { UMB_DYNAMIC_ROOT_PROPERTY_EDITOR_UI_ALIAS } from '@umbraco-cms/backoffice/content';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: UMB_DYNAMIC_ROOT_PROPERTY_EDITOR_UI_ALIAS,
	name: 'Dynamic Root Property Editor UI',
	element: () => import('./property-editor-ui-dynamic-root.element.js'),
	meta: {
		label: 'Dynamic Root',
		icon: 'icon-tree',
		group: '#propertyEditorUIGroups_advanced',
	},
};
