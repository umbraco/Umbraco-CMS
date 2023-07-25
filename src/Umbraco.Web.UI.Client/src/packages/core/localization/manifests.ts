import { ManifestTypes } from '../extension-registry/index.js';
import { ManifestTranslations } from '../extension-registry/models/translations.model.js';

const translationManifests: Array<ManifestTranslations> = [
	{
		type: 'translations',
		alias: 'Umb.Translations.En',
		weight: -100,
		name: 'English (UK)',
		meta: {
			culture: 'en',
		},
		loader: () => import('./lang/en.json'),
	},
];

export const manifests: Array<ManifestTypes> = [...translationManifests];
