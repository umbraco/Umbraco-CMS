import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.BlockGrid',
	name: 'Block Grid Property Editor UI',
	js: () => import('./property-editor-ui-block-grid.element.js'),
	meta: {
		label: 'Block Grid',
		propertyEditorSchemaAlias: 'Umbraco.BlockGrid',
		icon: 'icon-layout',
		group: 'richContent',
		settings: {
			properties: [
				{
					alias: 'useLiveEditing',
					label: 'Live editing mode',
					description: 'Live update content when editing in overlay',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'maxPropertyWidth',
					label: 'Editor width',
					description: 'Optional css overwrite. (example: 1200px or 100%)',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
				},
				{
					alias: 'createLabel',
					label: 'Create Button Label',
					description: 'Override the label text for adding a new block, Example Add Widget',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
				},
			],
		},
	},
};
