import { UMB_UNLOCK_USER_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const unlockRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_UNLOCK_USER_REPOSITORY_ALIAS,
	name: 'Unlock User Repository',
	api: () => import('./unlock-user.repository.js'),
};

export const manifests: Array<ManifestTypes> = [unlockRepository];
