import { DOCUMENT_TREE_ALIAS } from '../tree/manifests';
import type { ManifestModal } from '@umbraco-cms/backoffice/extensions-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		kind: 'treePicker',
		alias: 'Umb.Modal.DocumentPicker',
		name: 'Document Picker Modal',
		meta: {
			treeAlias: DOCUMENT_TREE_ALIAS,
		},
	},
];

export const manifests = [...modals];
