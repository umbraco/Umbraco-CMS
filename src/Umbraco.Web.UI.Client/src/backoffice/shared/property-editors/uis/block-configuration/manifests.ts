import type { ManifestPropertyEditorUI } from '@umbraco-cms/models';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.BlockConfiguration',
	name: 'Block Configuration Property Editor UI',
	loader: () => import('./property-editor-ui-block-configuration.element'),
	meta: {
		label: 'Block List',
		propertyEditorModel: '',
		icon: 'umb:autofill',
		group: 'common',
	},
};
