import type { UmbMockLanguageModel } from '../../types/mock-data-set.types.js';

export const data: Array<UmbMockLanguageModel> = [
	{
		name: 'English',
		isoCode: 'en-US',
		isDefault: true,
		isMandatory: true,
	},
	{
		name: 'Danish',
		isoCode: 'da',
		isDefault: false,
		isMandatory: false,
		fallbackIsoCode: 'en-US',
	},
	{
		name: 'Forbidden',
		isoCode: 'forbidden',
		isDefault: false,
		isMandatory: false,
	},
];
