import { UMB_DOCUMENT_ROLLBACK_REPOSITORY_ALIAS } from '../repository/constants.js';
import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS } from '../../repository/detail/constants.js';
import { UMB_DOCUMENT_ROLLBACK_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		kind: 'rollback',
		alias: UMB_DOCUMENT_ROLLBACK_MODAL_ALIAS,
		name: 'Document Rollback Modal',
		meta: {
			rollbackRepositoryAlias: UMB_DOCUMENT_ROLLBACK_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
		},
	},
];
