import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_TRACKED_REFERENCE_REPOSITORY_ALIAS = 'Umb.Repository.Media.TrackedReference';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_TRACKED_REFERENCE_REPOSITORY_ALIAS,
	name: 'Media Tracked Reference Repository',
	api: () => import('./media-tracked-reference.repository.js'),
};

export const manifests = [repository];
