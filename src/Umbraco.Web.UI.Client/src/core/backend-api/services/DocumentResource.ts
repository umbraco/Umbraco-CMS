/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DocumentTreeItem } from '../models/DocumentTreeItem';
import type { PagedDocumentTreeItem } from '../models/PagedDocumentTreeItem';
import type { PagedRecycleBinItem } from '../models/PagedRecycleBinItem';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DocumentResource {

    /**
     * @returns PagedRecycleBinItem Success
     * @throws ApiError
     */
    public static children({
        parentKey,
        skip,
        take = 100,
    }: {
        parentKey?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedRecycleBinItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/recycle-bin/document/children',
            query: {
                'parentKey': parentKey,
                'skip': skip,
                'take': take,
            },
            errors: {
                401: `Unauthorized`,
            },
        });
    }

    /**
     * @returns PagedRecycleBinItem Success
     * @throws ApiError
     */
    public static root({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedRecycleBinItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/recycle-bin/document/root',
            query: {
                'skip': skip,
                'take': take,
            },
            errors: {
                401: `Unauthorized`,
            },
        });
    }

    /**
     * @returns PagedDocumentTreeItem Success
     * @throws ApiError
     */
    public static children1({
        parentKey,
        skip,
        take = 100,
        dataTypeKey,
        culture,
    }: {
        parentKey?: string,
        skip?: number,
        take?: number,
        dataTypeKey?: string,
        culture?: string,
    }): CancelablePromise<PagedDocumentTreeItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document/children',
            query: {
                'parentKey': parentKey,
                'skip': skip,
                'take': take,
                'dataTypeKey': dataTypeKey,
                'culture': culture,
            },
        });
    }

    /**
     * @returns DocumentTreeItem Success
     * @throws ApiError
     */
    public static items({
        key,
        dataTypeKey,
        culture,
    }: {
        key?: Array<string>,
        dataTypeKey?: string,
        culture?: string,
    }): CancelablePromise<Array<DocumentTreeItem>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document/item',
            query: {
                'key': key,
                'dataTypeKey': dataTypeKey,
                'culture': culture,
            },
        });
    }

    /**
     * @returns PagedDocumentTreeItem Success
     * @throws ApiError
     */
    public static root1({
        skip,
        take = 100,
        dataTypeKey,
        culture,
    }: {
        skip?: number,
        take?: number,
        dataTypeKey?: string,
        culture?: string,
    }): CancelablePromise<PagedDocumentTreeItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document/root',
            query: {
                'skip': skip,
                'take': take,
                'dataTypeKey': dataTypeKey,
                'culture': culture,
            },
        });
    }

}
