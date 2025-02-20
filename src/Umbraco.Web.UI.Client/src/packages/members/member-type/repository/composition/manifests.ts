export const UMB_MEMBER_TYPE_COMPOSITION_REPOSITORY_ALIAS = 'Umb.Repository.MemberType.Composition';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_TYPE_COMPOSITION_REPOSITORY_ALIAS,
		name: 'Member Type Composition Repository',
		api: () => import('./member-type-composition.repository.js'),
	},
];
