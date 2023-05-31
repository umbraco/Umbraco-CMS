import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.BlockGrid.GroupConfiguration',
	name: 'Block Grid Group Configuration Property Editor UI',
	loader: () => import('./property-editor-ui-block-grid-group-configuration.element.js'),
	meta: {
		label: 'Block Grid Group Configuration',
		propertyEditorAlias: 'Umbraco.BlockGrid.GroupConfiguration',
		icon: 'umb:autofill',
		group: 'blocks',
	},
};
