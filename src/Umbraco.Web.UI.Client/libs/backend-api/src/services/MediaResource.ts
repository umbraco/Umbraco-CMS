/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ContentTreeItemModel } from '../models/ContentTreeItemModel';
import type { DocumentTreeItemModel } from '../models/DocumentTreeItemModel';
import type { PagedContentTreeItemModel } from '../models/PagedContentTreeItemModel';
import type { PagedRecycleBinItemModel } from '../models/PagedRecycleBinItemModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MediaResource {

    /**
     * @returns PagedRecycleBinItemModel Success
     * @throws ApiError
     */
    public static getRecycleBinMediaChildren({
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
     * @returns PagedRecycleBinItemModel Success
     * @throws ApiError
     */
    public static getRecycleBinMediaRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedRecycleBinItemModel> {
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
     * @returns PagedContentTreeItemModel Success
     * @throws ApiError
     */
    public static getTreeMediaChildren({
        parentKey,
        skip,
        take = 100,
        dataTypeKey,
    }: {
        parentKey?: string,
        skip?: number,
        take?: number,
        dataTypeKey?: string,
    }): CancelablePromise<PagedContentTreeItemModel> {
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
     * @returns any Success
     * @throws ApiError
     */
    public static getTreeMediaItem({
        key,
        dataTypeKey,
    }: {
        key?: Array<string>,
        dataTypeKey?: string,
    }): CancelablePromise<Array<(ContentTreeItemModel | DocumentTreeItemModel)>> {
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
     * @returns PagedContentTreeItemModel Success
     * @throws ApiError
     */
    public static getTreeMediaRoot({
        skip,
        take = 100,
        dataTypeKey,
    }: {
        skip?: number,
        take?: number,
        dataTypeKey?: string,
    }): CancelablePromise<PagedContentTreeItemModel> {
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
