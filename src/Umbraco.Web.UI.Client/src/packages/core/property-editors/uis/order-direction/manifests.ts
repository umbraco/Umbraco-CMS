import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.OrderDirection',
	name: 'Order Direction Property Editor UI',
	loader: () => import('./property-editor-ui-order-direction.element.js'),
	meta: {
		label: 'Order Direction',
		propertyEditorModel: '',
		icon: 'umb:autofill',
		group: 'common',
	},
};
