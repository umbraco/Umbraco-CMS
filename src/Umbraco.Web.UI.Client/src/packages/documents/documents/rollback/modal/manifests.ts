import { UMB_ROLLBACK_MODAL_ALIAS } from './constants.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'modal',
		alias: UMB_ROLLBACK_MODAL_ALIAS,
		name: 'Document Rollback Modal',
		js: () => import('./rollback-modal.element.js'),
	},
];
