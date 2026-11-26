import { UMB_DOCUMENT_DYNAMIC_ROOT_PROPERTY_EDITOR_UI_ALIAS } from '../constants.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: UMB_DOCUMENT_DYNAMIC_ROOT_PROPERTY_EDITOR_UI_ALIAS,
	name: 'Document Dynamic Root Property Editor UI',
	element: () => import('./property-editor-ui-document-dynamic-root.element.js'),
	meta: {
		label: 'Document Dynamic Root',
		icon: 'icon-tree',
		group: '#propertyEditorUIGroups_advanced',
	},
};
