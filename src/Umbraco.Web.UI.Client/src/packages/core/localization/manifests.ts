import type { ManifestLocalization } from '../extension-registry/models/localization.model.js';

const localizationManifests: Array<ManifestLocalization> = [
	{
		type: 'localization',
		alias: 'Umb.Localization.En_US',
		weight: -100,
		name: 'English (US)',
		meta: {
			culture: 'en-us',
		},
		js: () => import('../../../assets/lang/en-us.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Da_DK',
		weight: -100,
		name: 'Dansk (Danmark)',
		meta: {
			culture: 'da-dk',
		},
		js: () => import('../../../assets/lang/da-dk.js'),
	},
];

export const manifests = [...localizationManifests];
