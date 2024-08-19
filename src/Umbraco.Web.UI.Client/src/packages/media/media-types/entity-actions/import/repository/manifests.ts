import { UMB_MEDIA_TYPE_IMPORT_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const importRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_TYPE_IMPORT_REPOSITORY_ALIAS,
	name: 'Import Media Type Repository',
	api: () => import('./media-type-import.repository.js'),
};

export const manifests: Array<ManifestTypes> = [importRepository];
