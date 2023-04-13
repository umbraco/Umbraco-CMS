export const name = 'Umbraco.Core.TranslationManagement';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'Translation Entry Point',
		alias: 'Umb.EntryPoint.Translation',
		type: 'entryPoint',
		loader: () => import('./index'),
	},
];
