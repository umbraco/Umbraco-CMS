import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.NumberRange',
	name: 'Number Range Property Editor UI',
	loader: () => import('./property-editor-ui-number-range.element.js'),
	meta: {
		label: 'Number Range',
		propertyEditorAlias: '',
		icon: 'umb:autofill',
		group: 'common',
	},
};
