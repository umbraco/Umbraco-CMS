import { UMB_MOVE_DOCUMENT_TYPE_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const moveRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MOVE_DOCUMENT_TYPE_REPOSITORY_ALIAS,
	name: 'Move Document Type Repository',
	api: () => import('./document-type-move.repository.js'),
};

export const manifests: Array<ManifestTypes> = [moveRepository];
