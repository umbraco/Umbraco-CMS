import { UMB_DOCUMENT_TYPE_IMPORT_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const importRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_TYPE_IMPORT_REPOSITORY_ALIAS,
	name: 'Import Document Type Repository',
	api: () => import('./document-type-import.repository.js'),
};

export const manifests: Array<ManifestTypes> = [importRepository];
