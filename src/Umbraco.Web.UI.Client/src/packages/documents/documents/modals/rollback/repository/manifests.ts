import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_ROLLBACK_REPOSITORY_ALIAS = 'Umb.Repository.Rollback';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_ROLLBACK_REPOSITORY_ALIAS,
	name: 'Rollback Repository',
	api: () => import('./rollback.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
