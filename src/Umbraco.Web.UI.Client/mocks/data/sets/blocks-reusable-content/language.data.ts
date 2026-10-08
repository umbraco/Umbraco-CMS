import type { UmbMockLanguageModel } from '../../mock-data-set.types.js';

const englishLanguage: UmbMockLanguageModel = {
	name: 'English',
	isoCode: 'en-US',
	isDefault: true,
	isMandatory: true,
};

const danishLanguage: UmbMockLanguageModel = {
	name: 'Danish',
	isoCode: 'da',
	isDefault: false,
	isMandatory: false,
	fallbackIsoCode: 'en-US',
};

export const data: Array<UmbMockLanguageModel> = [englishLanguage, danishLanguage];
