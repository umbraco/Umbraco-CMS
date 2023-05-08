import { DOCUMENT_TYPE_TREE_ALIAS } from '../tree/manifests';
import type { ManifestModal } from '@umbraco-cms/backoffice/extensions-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		kind: 'treePicker',
		alias: 'Umb.Modal.DocumentTypePicker',
		name: 'Document Type Picker Modal',
		meta: {
			treeAlias: DOCUMENT_TYPE_TREE_ALIAS,
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.AllowedDocumentTypes',
		name: 'Allowed Document Types Modal',
		loader: () => import('./allowed-document-types/allowed-document-types-modal.element'),
	},
];

export const manifests = [...modals];
