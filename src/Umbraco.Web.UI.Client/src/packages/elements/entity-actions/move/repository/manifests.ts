import { UMB_MOVE_ELEMENT_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

const moveRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MOVE_ELEMENT_REPOSITORY_ALIAS,
	name: 'Move Element Repository',
	api: () => import('./element-move.repository.js'),
};

export const manifests = [moveRepository];
