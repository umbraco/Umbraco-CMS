import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ValueType',
	name: 'Value Type Property Editor UI',
	element: () => import('./property-editor-ui-value-type.element.js'),
	meta: {
		label: 'Value Type',
		icon: 'icon-autofill',
		group: 'common',
	},
};
