import type { ManifestPropertyEditorUI } from '@umbraco-cms/models';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.Tags',
	name: 'Tags Property Editor UI',
	loader: () => import('./property-editor-ui-tags.element'),
	meta: {
		label: 'Tags',
		propertyEditorModel: 'Umbraco.Tags',
		icon: 'umb:tags',
		group: 'common',
	},
};
