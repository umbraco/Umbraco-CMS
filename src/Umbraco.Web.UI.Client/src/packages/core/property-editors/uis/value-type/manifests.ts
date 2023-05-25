import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.ValueType',
	name: 'Value Type Property Editor UI',
	loader: () => import('./property-editor-ui-value-type.element.js'),
	meta: {
		label: 'Value Type',
		icon: 'umb:autofill',
		group: 'common',
		propertyEditorModel: 'Umbraco.JSON',
	},
};
