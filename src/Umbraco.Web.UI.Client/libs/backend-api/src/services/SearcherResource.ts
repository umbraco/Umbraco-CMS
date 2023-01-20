/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedSearcher } from '../models/PagedSearcher';
import type { PagedSearchResult } from '../models/PagedSearchResult';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class SearcherResource {

    /**
     * @returns PagedSearcher Success
     * @throws ApiError
     */
    public static getSearcher({
        skip,
        take,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedSearcher> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/searcher',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns PagedSearchResult Success
     * @throws ApiError
     */
    public static getSearcherBySearcherNameQuery({
        searcherName,
        term,
        skip,
        take,
    }: {
        searcherName: string,
        term?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedSearchResult> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/searcher/{searcherName}/query',
            path: {
                'searcherName': searcherName,
            },
            query: {
                'term': term,
                'skip': skip,
                'take': take,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

}
