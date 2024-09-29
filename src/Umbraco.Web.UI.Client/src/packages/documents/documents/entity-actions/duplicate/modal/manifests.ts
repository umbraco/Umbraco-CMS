import { UMB_DUPLICATE_DOCUMENT_MODAL_ALIAS } from './constants.js';
import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_DUPLICATE_DOCUMENT_MODAL_ALIAS,
		name: 'Duplicate Document To Modal',
		js: () => import('./duplicate-document-modal.element.js'),
	},
];
