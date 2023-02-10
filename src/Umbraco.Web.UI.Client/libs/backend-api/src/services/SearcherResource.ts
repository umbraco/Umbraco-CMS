/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedSearcherModel } from '../models/PagedSearcherModel';
import type { PagedSearchResultModel } from '../models/PagedSearchResultModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class SearcherResource {

    /**
     * @returns PagedSearcherModel Success
     * @throws ApiError
     */
    public static getSearcher({
        skip,
        take,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedSearcherModel> {
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
     * @returns PagedSearchResultModel Success
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
    }): CancelablePromise<PagedSearchResultModel> {
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
