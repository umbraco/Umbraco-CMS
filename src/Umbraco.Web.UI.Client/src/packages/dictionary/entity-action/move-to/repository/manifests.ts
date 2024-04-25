import { UMB_MOVE_DICTIONARY_REPOSITORY_ALIAS } from './constants.js';
import { UmbMoveDictionaryRepository } from './dictionary-move.repository.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const moveRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MOVE_DICTIONARY_REPOSITORY_ALIAS,
	name: 'Move Dictionary Repository',
	api: UmbMoveDictionaryRepository,
};

export const manifests: Array<ManifestTypes> = [moveRepository];
