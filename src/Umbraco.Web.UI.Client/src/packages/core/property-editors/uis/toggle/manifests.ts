import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.Toggle',
	name: 'Toggle Property Editor UI',
	loader: () => import('./property-editor-ui-toggle.element.js'),
	meta: {
		label: 'Toggle',
		propertyEditorAlias: 'Umbraco.TrueFalse',
		icon: 'umb:checkbox',
		group: 'common',
		settings: {
			properties: [
				{
					alias: 'default',
					label: 'Initial State',
					description:
						'The initial state for the toggle, when it is displayed for the first time in the backoffice, eg. for a new content item.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'showLabels',
					label: 'Show toggle labels',
					description: 'Show labels next to toggle button.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'labelOn',
					label: 'Label On',
					description: 'Label text when enabled.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
				},
				{
					alias: 'labelOff',
					label: 'Label Off',
					description: 'Label text when disabled.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
				},
			],
		},
	},
};
