import { UMB_DUPLICATE_DOCUMENT_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const duplicateRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DUPLICATE_DOCUMENT_REPOSITORY_ALIAS,
	name: 'Duplicate Document Repository',
	api: () => import('./document-duplicate.repository.js'),
};

export const manifests: Array<ManifestTypes> = [duplicateRepository];
