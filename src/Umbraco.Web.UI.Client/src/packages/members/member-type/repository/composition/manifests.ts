import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_TYPE_COMPOSITION_REPOSITORY_ALIAS = 'Umb.Repository.MemberType.Composition';

const compositionRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEMBER_TYPE_COMPOSITION_REPOSITORY_ALIAS,
	name: 'Member Type Composition Repository',
	api: () => import('./member-type-composition.repository.js'),
};

export const manifests: Array<ManifestTypes> = [compositionRepository];
