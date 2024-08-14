import { UMB_EXPORT_DOCUMENT_TYPE_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const exportRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_EXPORT_DOCUMENT_TYPE_REPOSITORY_ALIAS,
	name: 'Export Document Type Repository',
	api: () => import('./document-type-export.repository.js'),
};

export const manifests: Array<ManifestTypes> = [exportRepository];
