/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DocumentTreeItemModel } from '../models/DocumentTreeItemModel';
import type { PagedDocumentTreeItemModel } from '../models/PagedDocumentTreeItemModel';
import type { PagedRecycleBinItemModel } from '../models/PagedRecycleBinItemModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DocumentResource {

    /**
     * @returns PagedRecycleBinItemModel Success
     * @throws ApiError
     */
    public static getRecycleBinDocumentChildren({
        parentKey,
        skip,
        take = 100,
    }: {
        parentKey?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedRecycleBinItemModel> {
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
     * @returns PagedRecycleBinItemModel Success
     * @throws ApiError
     */
    public static getRecycleBinDocumentRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedRecycleBinItemModel> {
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
     * @returns PagedDocumentTreeItemModel Success
     * @throws ApiError
     */
    public static getTreeDocumentChildren({
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
    }): CancelablePromise<PagedDocumentTreeItemModel> {
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
     * @returns any Success
     * @throws ApiError
     */
    public static getTreeDocumentItem({
        key,
        dataTypeKey,
        culture,
    }: {
        key?: Array<string>,
        dataTypeKey?: string,
        culture?: string,
    }): CancelablePromise<Array<DocumentTreeItemModel>> {
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
     * @returns PagedDocumentTreeItemModel Success
     * @throws ApiError
     */
    public static getTreeDocumentRoot({
        skip,
        take = 100,
        dataTypeKey,
        culture,
    }: {
        skip?: number,
        take?: number,
        dataTypeKey?: string,
        culture?: string,
    }): CancelablePromise<PagedDocumentTreeItemModel> {
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
