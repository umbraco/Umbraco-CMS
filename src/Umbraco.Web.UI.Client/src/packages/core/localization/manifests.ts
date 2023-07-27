import { ManifestTypes } from '../extension-registry/index.js';
import { ManifestTranslations } from '../extension-registry/models/translations.model.js';

const translationManifests: Array<ManifestTranslations> = [
	{
		type: 'translations',
		alias: 'Umb.Translations.En_US',
		weight: -100,
		name: 'English (UK)',
		meta: {
			culture: 'en_us',
		},
		loader: () => import('../../../assets/lang/en_us.json'),
	},
];

export const manifests: Array<ManifestTypes> = [...translationManifests];
