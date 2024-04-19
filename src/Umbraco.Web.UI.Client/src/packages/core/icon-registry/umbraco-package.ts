export const name = 'Umbraco.Core.Icons';
export const extensions = [
	{
		type: 'icons',
		alias: 'Umb.Icons.Backoffice',
		name: 'Backoffice Icons',
		js: () => import('./icons/icons.js'),
	},
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.Icons',
		name: 'Icons Context',
		api: () => import('./icon-registry.context.js'),
	},
];
