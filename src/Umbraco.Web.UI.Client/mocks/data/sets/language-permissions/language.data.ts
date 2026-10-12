import type { UmbMockLanguageModel } from '../../mock-data-set.types.js';

export const ENGLISH_ISO_CODE = 'en-US';
export const DANISH_ISO_CODE = 'da';
export const VIETNAMESE_ISO_CODE = 'vi';

export const data: Array<UmbMockLanguageModel> = [
	{
		name: 'English (default language)',
		isoCode: ENGLISH_ISO_CODE,
		isDefault: true,
		isMandatory: true,
	},
	{
		name: 'Danish',
		isoCode: DANISH_ISO_CODE,
		isDefault: false,
		isMandatory: false,
		fallbackIsoCode: ENGLISH_ISO_CODE,
	},
	{
		name: 'Vietnamese',
		isoCode: VIETNAMESE_ISO_CODE,
		isDefault: false,
		isMandatory: false,
		fallbackIsoCode: ENGLISH_ISO_CODE,
	},
];
