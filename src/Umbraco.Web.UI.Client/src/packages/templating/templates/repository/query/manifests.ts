export const UMB_TEMPLATE_QUERY_REPOSITORY_ALIAS = 'Umb.Repository.TemplateQuery';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_TEMPLATE_QUERY_REPOSITORY_ALIAS,
		name: 'Template Query Repository',
		api: () => import('./template-query.repository.js'),
	},
];
