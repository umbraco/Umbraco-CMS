import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUi.Label',
	name: 'Label Property Editor UI',
	loader: () => import('./property-editor-ui-label.element.js'),
	meta: {
		label: 'Label',
		icon: 'umb:readonly',
		group: 'pickers',
		propertyEditorModel: 'Umbraco.Label',
	},
};
