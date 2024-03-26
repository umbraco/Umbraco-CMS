import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'BlockTypeGroupConfiguration',
	name: 'Block Group Configuration Property Editor UI',
	js: () => import('./property-editor-ui-block-type-group-configuration.element.js'),
	meta: {
		label: 'Block Grid Group Configuration',
		icon: 'icon-autofill',
		group: 'blocks',
	},
};
