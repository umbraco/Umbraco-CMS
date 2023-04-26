export const name = 'Umbraco.Core.UmbracoNews';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'Umbraco News Entry Point',
		alias: 'Umb.EntryPoint.UmbracoNews',
		type: 'entryPoint',
		loader: () => import('./index'),
	},
];
