import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.OverlaySize',
	name: 'Overlay Size Property Editor UI',
	loader: () => import('./property-editor-ui-overlay-size.element'),
	meta: {
		label: 'Overlay Size',
		propertyEditorModel: '',
		icon: 'umb:document',
		group: '',
	},
};
