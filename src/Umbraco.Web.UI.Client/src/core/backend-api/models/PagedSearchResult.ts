/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { SearchResult } from './SearchResult';

export type PagedSearchResult = {
    total?: number;
    items?: Array<SearchResult> | null;
};

