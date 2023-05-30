import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.NumberRange',
	name: 'Number Range Property Editor UI',
	loader: () => import('./property-editor-ui-number-range.element.js'),
	meta: {
		label: 'Number Range',
		propertyEditorModel: '',
		icon: 'umb:autofill',
		group: 'common',
	},
};
