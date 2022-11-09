/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { PagedSearchResult } from './PagedSearchResult';

export type PagedPaged = {
    total?: number;
    items?: Array<PagedSearchResult> | null;
};

