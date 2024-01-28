import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.BlockTypeGroupConfiguration',
	name: 'Block Grid Group Configuration Property Editor UI',
	js: () => import('./property-editor-ui-block-grid-group-configuration.element.js'),
	meta: {
		label: '',
		icon: 'icon-box-alt',
		group: 'common',
	},
};
