/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedSearcherResponseModel } from '../models/PagedSearcherResponseModel';
import type { PagedSearchResultResponseModel } from '../models/PagedSearchResultResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class SearcherResource {

    /**
     * @returns PagedSearcherResponseModel Success
     * @throws ApiError
     */
    public static getSearcher({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedSearcherResponseModel> {
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
     * @returns PagedSearchResultResponseModel Success
     * @throws ApiError
     */
    public static getSearcherBySearcherNameQuery({
        searcherName,
        term,
        skip,
        take = 100,
    }: {
        searcherName: string,
        term?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedSearchResultResponseModel> {
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
