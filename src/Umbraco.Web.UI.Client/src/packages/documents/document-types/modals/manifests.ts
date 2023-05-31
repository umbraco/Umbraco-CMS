import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.AllowedDocumentTypes',
		name: 'Allowed Document Types Modal',
		loader: () => import('./allowed-document-types/allowed-document-types-modal.element.js'),
	},
];

export const manifests = [...modals];
