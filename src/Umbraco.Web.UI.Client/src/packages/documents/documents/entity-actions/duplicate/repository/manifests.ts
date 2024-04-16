import { UMB_DUPLICATE_DOCUMENT_REPOSITORY_ALIAS } from './constants.js';
import { UmbDuplicateDocumentRepository } from './document-duplicate.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

const duplicateRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DUPLICATE_DOCUMENT_REPOSITORY_ALIAS,
	name: 'Duplicate Document Repository',
	api: UmbDuplicateDocumentRepository,
};

export const manifests = [duplicateRepository];
