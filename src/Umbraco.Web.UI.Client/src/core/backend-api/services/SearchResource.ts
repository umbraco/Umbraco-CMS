/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { Index } from '../models/Index';
import type { OkResult } from '../models/OkResult';
import type { PagedIndex } from '../models/PagedIndex';
import type { PagedPaged } from '../models/PagedPaged';
import type { PagedSearcher } from '../models/PagedSearcher';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class SearchResource {

    /**
     * @returns PagedIndex Success
     * @throws ApiError
     */
    public static getUmbracoManagementApiV1SearchIndex({
        skip,
        take,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedIndex> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/search/index',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns Index Success
     * @throws ApiError
     */
    public static getUmbracoManagementApiV1SearchIndex1({
        indexName,
    }: {
        indexName: string,
    }): CancelablePromise<Index> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/search/index/{indexName}',
            path: {
                'indexName': indexName,
            },
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns OkResult Success
     * @throws ApiError
     */
    public static postUmbracoManagementApiV1SearchIndexRebuild({
        indexName,
    }: {
        indexName: string,
    }): CancelablePromise<OkResult> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/search/index/{indexName}/rebuild',
            path: {
                'indexName': indexName,
            },
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns PagedSearcher Success
     * @throws ApiError
     */
    public static getUmbracoManagementApiV1SearchSearcher({
        skip,
        take,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedSearcher> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/search/searcher',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns PagedPaged Success
     * @throws ApiError
     */
    public static getUmbracoManagementApiV1SearchSearcherSearch({
        searcherName,
        query,
        skip,
        take,
    }: {
        searcherName: string,
        query?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedPaged> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/search/searcher/{searcherName}/search',
            path: {
                'searcherName': searcherName,
            },
            query: {
                'query': query,
                'skip': skip,
                'take': take,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

}
