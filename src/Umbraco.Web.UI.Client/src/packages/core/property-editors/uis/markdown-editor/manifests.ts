import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.MarkdownEditor',
	name: 'Markdown Editor Property Editor UI',
	loader: () => import('./property-editor-ui-markdown-editor.element.js'),
	meta: {
		label: 'Markdown Editor',
		propertyEditorModel: 'Umbraco.MarkdownEditor',
		icon: 'umb:code',
		group: 'pickers',
		settings: {
			properties: [
				{
					alias: 'preview',
					label: 'Preview',
					description: 'Display a live preview',
					propertyEditorUI: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'defaultValue',
					label: 'Default value',
					description: 'If value is blank, the editor will show this',
					propertyEditorUI: 'Umb.PropertyEditorUi.TextArea',
				},
				{
					alias: 'overlaySize',
					label: 'Overlay Size',
					description: 'Select the width of the overlay.',
					propertyEditorUI: 'Umb.PropertyEditorUi.OverlaySize',
				},
			],
		},
	},
};
