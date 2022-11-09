/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ContentTreeItem } from '../models/ContentTreeItem';
import type { PagedContentTreeItem } from '../models/PagedContentTreeItem';
import type { PagedRecycleBinItem } from '../models/PagedRecycleBinItem';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MediaResource {

    /**
     * @returns PagedRecycleBinItem Success
     * @throws ApiError
     */
    public static getUmbracoManagementApiV1RecycleBinMediaChildren({
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
            url: '/umbraco/management/api/v1/recycle-bin/media/children',
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
    public static getUmbracoManagementApiV1RecycleBinMediaRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedRecycleBinItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/recycle-bin/media/root',
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
     * @returns PagedContentTreeItem Success
     * @throws ApiError
     */
    public static getUmbracoManagementApiV1TreeMediaChildren({
        parentKey,
        skip,
        take = 100,
        dataTypeKey,
    }: {
        parentKey?: string,
        skip?: number,
        take?: number,
        dataTypeKey?: string,
    }): CancelablePromise<PagedContentTreeItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media/children',
            query: {
                'parentKey': parentKey,
                'skip': skip,
                'take': take,
                'dataTypeKey': dataTypeKey,
            },
        });
    }

    /**
     * @returns ContentTreeItem Success
     * @throws ApiError
     */
    public static getUmbracoManagementApiV1TreeMediaItem({
        key,
        dataTypeKey,
    }: {
        key?: Array<string>,
        dataTypeKey?: string,
    }): CancelablePromise<Array<ContentTreeItem>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media/item',
            query: {
                'key': key,
                'dataTypeKey': dataTypeKey,
            },
        });
    }

    /**
     * @returns PagedContentTreeItem Success
     * @throws ApiError
     */
    public static getUmbracoManagementApiV1TreeMediaRoot({
        skip,
        take = 100,
        dataTypeKey,
    }: {
        skip?: number,
        take?: number,
        dataTypeKey?: string,
    }): CancelablePromise<PagedContentTreeItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media/root',
            query: {
                'skip': skip,
                'take': take,
                'dataTypeKey': dataTypeKey,
            },
        });
    }

}
