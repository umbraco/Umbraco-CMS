import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extensions-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.Label',
	name: 'Label Property Editor UI',
	loader: () => import('./property-editor-ui-label.element'),
	meta: {
		label: 'Label',
		icon: 'umb:readonly',
		group: 'pickers',
		propertyEditorModel: 'Umbraco.Label',
	},
};
