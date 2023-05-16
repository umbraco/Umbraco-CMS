import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.Tags.StorageType',
	name: 'Tags Storage Type Property Editor UI',
	loader: () => import('./property-editor-ui-tags-storage-type.element'),
	meta: {
		label: 'Tags Storage Type',
		propertyEditorModel: '',
		icon: 'umb:autofill',
		group: 'common',
	},
};
