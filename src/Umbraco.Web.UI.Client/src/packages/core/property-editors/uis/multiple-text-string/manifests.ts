import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.MultipleTextString',
	name: 'Multiple Text String Property Editor UI',
	loader: () => import('./property-editor-ui-multiple-text-string.element.js'),
	meta: {
		label: 'Multiple Text String',
		propertyEditorModel: 'Umbraco.MultipleTextString',
		icon: 'umb:ordered-list',
		group: '',
		supportsReadOnly: true,
	},
};
