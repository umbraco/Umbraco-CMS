import { UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const queryRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
	name: 'Document Recycle Bin Repository',
	api: () => import('./document-recycle-bin.repository.js'),
};

export const manifests: Array<ManifestTypes> = [queryRepository];
