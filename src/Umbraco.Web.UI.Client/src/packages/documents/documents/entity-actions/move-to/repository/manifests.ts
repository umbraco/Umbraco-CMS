import { UMB_MOVE_DOCUMENT_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const moveRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MOVE_DOCUMENT_REPOSITORY_ALIAS,
	name: 'Move Document Repository',
	api: () => import('./document-move.repository.js'),
};

export const manifests: Array<ManifestTypes> = [moveRepository];
