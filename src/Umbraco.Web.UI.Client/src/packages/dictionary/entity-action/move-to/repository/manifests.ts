import { UMB_MOVE_DICTIONARY_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const moveRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MOVE_DICTIONARY_REPOSITORY_ALIAS,
	name: 'Move Dictionary Repository',
	api: () => import('./dictionary-move.repository.js'),
};

export const manifests: Array<ManifestTypes> = [moveRepository];
