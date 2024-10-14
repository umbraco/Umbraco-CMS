import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.BlockTypeGroupConfiguration',
	name: 'Block Grid Group Configuration Property Editor UI',
	element: () => import('./property-editor-ui-block-grid-group-configuration.element.js'),
	meta: {
		label: '',
		icon: 'icon-box-alt',
		group: 'common',
	},
};
