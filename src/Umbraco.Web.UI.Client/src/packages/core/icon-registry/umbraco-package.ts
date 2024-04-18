export const name = 'Umbraco.Core.Icons';
export const extensions = [
	{
		name: 'Backoffice Icons',
		alias: 'Umb.Icons.Backoffice',
		type: 'icons',
		js: () => import('./icons/icons.js'),
	},
];
