import { UmbMockCultureItemManager } from '../utils/culture/culture-item.manager.js';
import { UmbCultureMockDbBase } from '../utils/culture/culture-base.js';
import { UmbMockCultureDetailManager } from '../utils/culture/culture-detail.manager.js';
import type { UmbMockLanguageModel } from './language.data.js';
import { data } from './language.data.js';
import type {
	CreateLanguageRequestModel,
	LanguageItemResponseModel,
	LanguageResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

class UmbLanguageMockDB extends UmbCultureMockDbBase<UmbMockLanguageModel> {
	item = new UmbMockCultureItemManager<UmbMockLanguageModel>(this, itemResponseMapper);
	detail = new UmbMockCultureDetailManager<UmbMockLanguageModel>(this, createDetailMockMapper, detailResponseMapper);

	constructor(data: Array<UmbMockLanguageModel>) {
		super(data);
	}
}

const createDetailMockMapper = (request: CreateLanguageRequestModel): UmbMockLanguageModel => {
	return {
		fallbackIsoCode: request.fallbackIsoCode,
		isDefault: request.isDefault,
		isMandatory: request.isMandatory,
		isoCode: request.isoCode,
		name: request.name,
	};
};

const detailResponseMapper = (item: UmbMockLanguageModel): LanguageResponseModel => {
	return {
		fallbackIsoCode: item.fallbackIsoCode,
		isDefault: item.isDefault,
		isMandatory: item.isMandatory,
		isoCode: item.isoCode,
		name: item.name,
	};
};

const itemResponseMapper = (item: UmbMockLanguageModel): LanguageItemResponseModel => {
	return {
		isoCode: item.isoCode,
		name: item.name,
	};
};

export const umbLanguageMockDb = new UmbLanguageMockDB(data);
