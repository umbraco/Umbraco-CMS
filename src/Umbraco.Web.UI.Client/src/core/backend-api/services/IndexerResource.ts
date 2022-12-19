/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { Index } from '../models/Index';
import type { OkResult } from '../models/OkResult';
import type { PagedIndex } from '../models/PagedIndex';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class IndexerResource {

    /**
     * @returns PagedIndex Success
     * @throws ApiError
     */
    public static getIndexer({
        skip,
        take,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedIndex> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/indexer',
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
    public static getIndexerByIndexName({
        indexName,
    }: {
        indexName: string,
    }): CancelablePromise<Index> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/indexer/{indexName}',
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
    public static postIndexerByIndexNameRebuild({
        indexName,
    }: {
        indexName: string,
    }): CancelablePromise<OkResult> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/indexer/{indexName}/rebuild',
            path: {
                'indexName': indexName,
            },
            errors: {
                400: `Bad Request`,
            },
        });
    }

}
