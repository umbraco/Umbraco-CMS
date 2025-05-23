export const manifest: UmbExtensionManifest = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUI.Dimensions',
	name: 'Dimensions Property Editor UI',
	element: () => import('./property-editor-ui-dimensions.element.js'),
	meta: {
		label: 'Dimensions',
		icon: 'icon-fullscreen-alt',
		group: 'common',
	},
};
