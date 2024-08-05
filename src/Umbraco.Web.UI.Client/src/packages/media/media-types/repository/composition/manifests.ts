import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_TYPE_COMPOSITION_REPOSITORY_ALIAS = 'Umb.Repository.MediaType.Composition';

const compositionRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_TYPE_COMPOSITION_REPOSITORY_ALIAS,
	name: 'Media Type Composition Repository',
	api: () => import('./media-type-composition.repository.js'),
};

export const manifests: Array<ManifestTypes> = [compositionRepository];
