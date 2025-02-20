import type { LanguageItemResponseModel, LanguageResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockLanguageModel = LanguageResponseModel & LanguageItemResponseModel;

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
];
