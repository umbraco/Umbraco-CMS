/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { SearchResultResponseModel } from './SearchResultResponseModel';

export type PagedSearchResultResponseModel = {
    total: number;
    items: Array<SearchResultResponseModel>;
};
