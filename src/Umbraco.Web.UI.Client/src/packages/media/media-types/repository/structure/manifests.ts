import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_TYPE_STRUCTURE_REPOSITORY_ALIAS = 'Umb.Repository.MediaType.Structure';

const structureRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_TYPE_STRUCTURE_REPOSITORY_ALIAS,
	name: 'Media Type Structure Repository',
	api: () => import('./media-type-structure.repository.js'),
};

export const manifests = [structureRepository];
