import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.BlockList.BlockConfiguration',
	name: 'Block List Block Configuration Property Editor UI',
	loader: () => import('./property-editor-ui-block-list-block-configuration.element'),
	meta: {
		label: 'Block List Block Configuration',
		propertyEditorModel: '',
		icon: 'umb:autofill',
		group: 'common',
	},
};
