import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.BlockList.BlockConfiguration',
	name: 'Block List Block Configuration Property Editor UI',
	loader: () => import('./property-editor-ui-block-list-block-configuration.element.js'),
	meta: {
		label: 'Block List Block Configuration',
		propertyEditorAlias: '',
		icon: 'umb:autofill',
		group: 'common',
	},
};
