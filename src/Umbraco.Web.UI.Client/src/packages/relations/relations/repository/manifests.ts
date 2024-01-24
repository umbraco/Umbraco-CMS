import { UmbRelationRepository } from './relation.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_RELATION_REPOSITORY_ALIAS = 'Umb.Repository.Relation';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_RELATION_REPOSITORY_ALIAS,
	name: 'Relation Repository',
	api: UmbRelationRepository,
};

export const manifests = [repository];
