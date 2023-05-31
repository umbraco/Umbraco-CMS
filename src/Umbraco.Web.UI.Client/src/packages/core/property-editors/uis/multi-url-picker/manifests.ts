import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.MultiUrlPicker',
	name: 'Multi URL Picker Property Editor UI',
	loader: () => import('./property-editor-ui-multi-url-picker.element.js'),
	meta: {
		label: 'Multi URL Picker',
		propertyEditorModel: 'Umbraco.MultiUrlPicker',
		icon: 'umb:link',
		group: 'pickers',
		settings: {
			properties: [
				{
					alias: 'overlaySize',
					label: 'Overlay Size',
					description: 'Select the width of the overlay.',
					propertyEditorUi: 'Umb.PropertyEditorUi.OverlaySize',
				},
				{
					alias: 'hideAnchor',
					label: 'Hide anchor/query string input',
					description: 'Selecting this hides the anchor/query string input field in the link picker overlay.',
					propertyEditorUi: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};
