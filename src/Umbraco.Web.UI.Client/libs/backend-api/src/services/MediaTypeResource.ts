/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DocumentTypeTreeItemModel } from '../models/DocumentTypeTreeItemModel';
import type { FolderTreeItemModel } from '../models/FolderTreeItemModel';
import type { PagedFolderTreeItemModel } from '../models/PagedFolderTreeItemModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MediaTypeResource {

    /**
     * @returns PagedFolderTreeItemModel Success
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
    }): CancelablePromise<PagedFolderTreeItemModel> {
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
     * @returns any Success
     * @throws ApiError
     */
    public static getTreeMediaTypeItem({
        key,
    }: {
        key?: Array<string>,
    }): CancelablePromise<Array<(FolderTreeItemModel | DocumentTypeTreeItemModel)>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media-type/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedFolderTreeItemModel Success
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
    }): CancelablePromise<PagedFolderTreeItemModel> {
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
