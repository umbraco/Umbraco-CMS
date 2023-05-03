export const name = 'Umbraco.Core.UmbracoNews';
export const extensions = [
	{
		name: 'Umbraco News Entry Point',
		alias: 'Umb.EntryPoint.UmbracoNews',
		type: 'entryPoint',
		loader: () => import('./index'),
	},
];
