import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const UMB_DUPLICATE_DOCUMENT_MODAL_ALIAS = 'Umb.Modal.Document.Duplicate';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_DUPLICATE_DOCUMENT_MODAL_ALIAS,
		name: 'Duplicate Document To Modal',
		element: () => import('./duplicate-document-modal.element.js'),
	},
];
