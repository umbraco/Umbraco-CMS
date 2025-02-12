import { UMB_MOVE_DOCUMENT_BLUEPRINT_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

const moveRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MOVE_DOCUMENT_BLUEPRINT_REPOSITORY_ALIAS,
	name: 'Move Document Blueprint Repository',
	api: () => import('./document-blueprint-move.repository.js'),
};

export const manifests = [moveRepository];
