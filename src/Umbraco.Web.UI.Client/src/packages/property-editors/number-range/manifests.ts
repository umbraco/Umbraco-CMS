import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.NumberRange',
	name: 'Number Range Property Editor UI',
	element: () => import('./property-editor-ui-number-range.element.js'),
	meta: {
		label: 'Number Range',
		icon: 'icon-autofill',
		group: 'common',
	},
};
