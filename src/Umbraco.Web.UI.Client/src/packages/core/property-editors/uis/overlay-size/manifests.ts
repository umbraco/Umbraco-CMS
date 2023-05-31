import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.OverlaySize',
	name: 'Overlay Size Property Editor UI',
	loader: () => import('./property-editor-ui-overlay-size.element.js'),
	meta: {
		label: 'Overlay Size',
		propertyEditorAlias: '',
		icon: 'umb:document',
		group: '',
	},
};
