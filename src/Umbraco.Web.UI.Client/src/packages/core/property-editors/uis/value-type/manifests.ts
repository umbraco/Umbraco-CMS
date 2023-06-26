import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ValueType',
	name: 'Value Type Property Editor UI',
	loader: () => import('./property-editor-ui-value-type.element.js'),
	meta: {
		label: 'Value Type',
		icon: 'umb:autofill',
		group: 'common',
		propertyEditorSchemaAlias: 'Umbraco.JSON',
	},
};
