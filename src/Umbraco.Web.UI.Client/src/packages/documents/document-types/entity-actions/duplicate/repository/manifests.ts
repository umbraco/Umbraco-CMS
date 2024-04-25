import { UMB_DUPLICATE_DOCUMENT_TYPE_REPOSITORY_ALIAS } from './constants.js';
import { UmbDuplicateDocumentTypeRepository } from './document-type-duplicate.repository.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const duplicateRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DUPLICATE_DOCUMENT_TYPE_REPOSITORY_ALIAS,
	name: 'Duplicate Document Type Repository',
	api: UmbDuplicateDocumentTypeRepository,
};

export const manifests: Array<ManifestTypes> = [duplicateRepository];
