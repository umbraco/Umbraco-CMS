import { dataSet } from '../sets/index.js';
import type { LanguageItemResponseModel, LanguageResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockLanguageModel = LanguageResponseModel & LanguageItemResponseModel;

export const data: Array<UmbMockLanguageModel> = dataSet.language;
