export const UMB_RELATION_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.Relation.Collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_RELATION_COLLECTION_REPOSITORY_ALIAS,
		name: 'Relation Collection Repository',
		api: () => import('./relation-collection.repository.js'),
	},
];
