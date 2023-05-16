import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.Toggle',
	name: 'Toggle Property Editor UI',
	loader: () => import('./property-editor-ui-toggle.element'),
	meta: {
		label: 'Toggle',
		propertyEditorModel: 'Umbraco.TrueFalse',
		icon: 'umb:checkbox',
		group: 'common',
		config: {
			properties: [
				{
					alias: 'default',
					label: 'Initial State',
					description:
						'The initial state for the toggle, when it is displayed for the first time in the backoffice, eg. for a new content item.',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
				{
					alias: 'showLabels',
					label: 'Show toggle labels',
					description: 'Show labels next to toggle button.',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
				{
					alias: 'labelOn',
					label: 'Label On',
					description: 'Label text when enabled.',
					propertyEditorUI: 'Umb.PropertyEditorUI.TextBox',
				},
				{
					alias: 'labelOff',
					label: 'Label Off',
					description: 'Label text when disabled.',
					propertyEditorUI: 'Umb.PropertyEditorUI.TextBox',
				},
			],
		},
	},
};
