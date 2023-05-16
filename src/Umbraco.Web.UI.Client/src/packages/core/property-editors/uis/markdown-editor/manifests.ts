import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.MarkdownEditor',
	name: 'Markdown Editor Property Editor UI',
	loader: () => import('./property-editor-ui-markdown-editor.element'),
	meta: {
		label: 'Markdown Editor',
		propertyEditorModel: 'Umbraco.MarkdownEditor',
		icon: 'umb:code',
		group: 'pickers',
		config: {
			properties: [
				{
					alias: 'preview',
					label: 'Preview',
					description: 'Display a live preview',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
				{
					alias: 'defaultValue',
					label: 'Default value',
					description: 'If value is blank, the editor will show this',
					propertyEditorUI: 'Umb.PropertyEditorUI.TextArea',
				},
				{
					alias: 'overlaySize',
					label: 'Overlay Size',
					description: 'Select the width of the overlay.',
					propertyEditorUI: 'Umb.PropertyEditorUI.OverlaySize',
				},
			],
		},
	},
};
