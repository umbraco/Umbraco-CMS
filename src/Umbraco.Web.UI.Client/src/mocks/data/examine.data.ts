import { dataSet } from './sets/index.js';
import type {
	IndexResponseModel,
	PagedIndexResponseModel,
	SearchResultResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export const getIndexByName = dataSet.examineGetIndexByName;

export const getSearchResultsMockData = dataSet.examineGetSearchResults;

export const Indexers: IndexResponseModel[] = dataSet.examineIndexers;

export const PagedIndexers: PagedIndexResponseModel = dataSet.examinePagedIndexers;

export const searchResultMockData: SearchResultResponseModel[] = dataSet.examineSearchResults;
