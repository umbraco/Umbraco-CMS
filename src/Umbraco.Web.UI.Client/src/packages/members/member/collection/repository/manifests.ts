import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.Member.Collection';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEMBER_COLLECTION_REPOSITORY_ALIAS,
	name: 'Member Collection Repository',
	api: () => import('./member-collection.repository.js'),
};

export const manifests = [repository];
