import type { UmbMockLanguageModel } from '../../mock-data-set.types.js';
import { CULTURE_DA, CULTURE_EN } from './ids.js';

export const data: Array<UmbMockLanguageModel> = [
	{
		name: 'English (United States)',
		isoCode: CULTURE_EN,
		isDefault: true,
		isMandatory: true,
	},
	{
		name: 'Danish (Denmark)',
		isoCode: CULTURE_DA,
		isDefault: false,
		isMandatory: false,
		fallbackIsoCode: CULTURE_EN,
	},
];
