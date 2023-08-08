/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { IndexResponseModel } from '../models/IndexResponseModel';
import type { OkResult } from '../models/OkResult';
import type { PagedIndexResponseModel } from '../models/PagedIndexResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class IndexerResource {

    /**
     * @returns PagedIndexResponseModel Success
     * @throws ApiError
     */
    public static getIndexer({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedIndexResponseModel> {
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
     * @returns any Success
     * @throws ApiError
     */
    public static getIndexerByIndexName({
        indexName,
    }: {
        indexName: string,
    }): CancelablePromise<IndexResponseModel> {
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
                404: `Not Found`,
                409: `Conflict`,
            },
        });
    }

}
