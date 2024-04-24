import { UMB_MOVE_DOCUMENT_REPOSITORY_ALIAS } from './constants.js';
import { UmbMoveDocumentRepository } from './document-move.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

const moveRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MOVE_DOCUMENT_REPOSITORY_ALIAS,
	name: 'Move Document Repository',
	api: UmbMoveDocumentRepository,
};

export const manifests: Array<ManifestTypes> = [moveRepository];
