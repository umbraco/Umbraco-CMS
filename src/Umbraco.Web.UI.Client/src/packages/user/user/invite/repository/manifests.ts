import { UMB_INVITE_USER_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const inviteRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_INVITE_USER_REPOSITORY_ALIAS,
	name: 'Invite User Repository',
	api: () => import('./invite-user.repository.js'),
};

export const manifests: Array<ManifestTypes> = [inviteRepository];
