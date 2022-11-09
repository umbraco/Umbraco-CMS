/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { Searcher } from './Searcher';

export type PagedSearcher = {
    total?: number;
    items?: Array<Searcher> | null;
};

