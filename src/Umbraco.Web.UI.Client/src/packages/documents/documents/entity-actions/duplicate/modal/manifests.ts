import { UMB_DUPLICATE_DOCUMENT_MODAL_ALIAS } from './constants.js';
import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_DUPLICATE_DOCUMENT_MODAL_ALIAS,
		name: 'Duplicate Document To Modal',
		element: () => import('./duplicate-document-modal.element.js'),
	},
];
