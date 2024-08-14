import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.DocumentType.Import',
		name: 'Document Type Import Modal',
		element: () => import('./document-type-import-modal.element.js'),
	},
];
