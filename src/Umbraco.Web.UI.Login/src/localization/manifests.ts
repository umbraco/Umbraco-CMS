export const manifests: UmbExtensionManifest[] = [
	{
		type: 'localization',
		alias: 'Umb.Auth.Localization.En',
		name: 'English',
		weight: 100,
		js: () => import('./lang/en.js'),
		meta: {
			culture: 'en',
		},
	},
	{
		type: 'localization',
		alias: 'Umb.Auth.Localization.EnUs',
		name: 'English (US)',
		weight: 100,
		js: () => import('./lang/en-us.js'),
		meta: {
			culture: 'en-US',
		},
	},
	{
		type: 'localization',
		alias: 'Umb.Auth.Localization.Da',
		name: 'Danish',
		weight: 100,
		js: () => import('./lang/da.js'),
		meta: {
			culture: 'da',
		},
	},
	{
		type: 'localization',
		alias: 'Umb.Auth.Localization.De',
		name: 'German',
		weight: 100,
		js: () => import('./lang/de.js'),
		meta: {
			culture: 'de',
		},
	},
	{
		type: 'localization',
		alias: 'Umb.Auth.Localization.Nb',
		name: 'Norwegian Bokmål',
		weight: 100,
		js: () => import('./lang/nb.js'),
		meta: {
			culture: 'nb',
		},
	},
	{
		type: 'localization',
		alias: 'Umb.Auth.Localization.Nl',
		name: 'Dutch',
		weight: 100,
		js: () => import('./lang/nl.js'),
		meta: {
			culture: 'nl',
		},
	},
	{
		type: 'localization',
		alias: 'Umb.Auth.Localization.Sv',
		name: 'Swedish',
		weight: 100,
		js: () => import('./lang/sv.js'),
		meta: {
			culture: 'sv',
		},
	},
];
