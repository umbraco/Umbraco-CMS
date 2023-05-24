import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.BlockGrid.GroupConfiguration',
	name: 'Block Grid Group Configuration Property Editor UI',
	loader: () => import('./property-editor-ui-block-grid-group-configuration.element.js'),
	meta: {
		label: 'Block Grid Group Configuration',
		propertyEditorModel: 'Umbraco.BlockGrid.GroupConfiguration',
		icon: 'umb:autofill',
		group: 'blocks',
	},
};
