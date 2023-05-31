import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
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
