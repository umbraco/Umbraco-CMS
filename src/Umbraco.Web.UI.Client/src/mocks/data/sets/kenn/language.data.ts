import type { UmbMockLanguageModel } from '../../types/mock-data-set.types.js';

export const data: Array<UmbMockLanguageModel> = [
	{
		"name": "English (United States)",
		"isoCode": "en-US",
		"isDefault": true,
		"isMandatory": true
	},
	{
		"name": "Danish (Denmark)",
		"isoCode": "da-dk",
		"isDefault": false,
		"isMandatory": false,
		"fallbackIsoCode": "en-US"
	}
];
