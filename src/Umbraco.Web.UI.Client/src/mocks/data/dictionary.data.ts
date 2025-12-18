import { dataSet } from './sets/index.js';
import type {
	DictionaryItemItemResponseModel,
	DictionaryItemResponseModel,
	DictionaryOverviewResponseModel,
	NamedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockDictionaryModel = DictionaryItemResponseModel &
	NamedEntityTreeItemResponseModel &
	DictionaryItemItemResponseModel &
	DictionaryOverviewResponseModel;

export const data: Array<UmbMockDictionaryModel> = dataSet.dictionary;
