import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.BlockGrid.BlockConfiguration',
	name: 'Block Grid Block Configuration Property Editor UI',
	loader: () => import('./property-editor-ui-block-grid-block-configuration.element.js'),
	meta: {
		label: 'Block Grid Block Configuration',
		propertyEditorSchemaAlias: 'Umbraco.BlockGrid.BlockConfiguration',
		icon: 'umb:autofill',
		group: 'blocks',
	},
};
