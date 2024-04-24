import { UMB_MOVE_DOCUMENT_TYPE_REPOSITORY_ALIAS } from './constants.js';
import { UmbMoveDocumentTypeRepository } from './document-type-move.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

const moveRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MOVE_DOCUMENT_TYPE_REPOSITORY_ALIAS,
	name: 'Move Document Type Repository',
	api: UmbMoveDocumentTypeRepository,
};

export const manifests: Array<ManifestTypes> = [moveRepository];
