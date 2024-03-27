import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_REFERENCE_REPOSITORY_ALIAS = 'Umb.Repository.Media.Reference';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_REFERENCE_REPOSITORY_ALIAS,
	name: 'Media Reference Repository',
	api: () => import('./media-reference.repository.js'),
};

export const manifests = [repository];
