/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { FolderTreeItem } from '../models/FolderTreeItem';
import type { PagedFolderTreeItem } from '../models/PagedFolderTreeItem';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MediaTypeResource {

    /**
     * @returns PagedFolderTreeItem Success
     * @throws ApiError
     */
    public static getTreeMediaTypeChildren({
        parentKey,
        skip,
        take = 100,
        foldersOnly = false,
    }: {
        parentKey?: string,
        skip?: number,
        take?: number,
        foldersOnly?: boolean,
    }): CancelablePromise<PagedFolderTreeItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media-type/children',
            query: {
                'parentKey': parentKey,
                'skip': skip,
                'take': take,
                'foldersOnly': foldersOnly,
            },
        });
    }

    /**
     * @returns FolderTreeItem Success
     * @throws ApiError
     */
    public static getTreeMediaTypeItem({
        key,
    }: {
        key?: Array<string>,
    }): CancelablePromise<Array<FolderTreeItem>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media-type/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedFolderTreeItem Success
     * @throws ApiError
     */
    public static getTreeMediaTypeRoot({
        skip,
        take = 100,
        foldersOnly = false,
    }: {
        skip?: number,
        take?: number,
        foldersOnly?: boolean,
    }): CancelablePromise<PagedFolderTreeItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media-type/root',
            query: {
                'skip': skip,
                'take': take,
                'foldersOnly': foldersOnly,
            },
        });
    }

}
