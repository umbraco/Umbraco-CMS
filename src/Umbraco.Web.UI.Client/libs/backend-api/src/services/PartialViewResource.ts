/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { FileSystemTreeItemModel } from '../models/FileSystemTreeItemModel';
import type { PagedFileSystemTreeItemModel } from '../models/PagedFileSystemTreeItemModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class PartialViewResource {

    /**
     * @returns PagedFileSystemTreeItemModel Success
     * @throws ApiError
     */
    public static getTreePartialViewChildren({
        path,
        skip,
        take = 100,
    }: {
        path?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedFileSystemTreeItemModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/partial-view/children',
            query: {
                'path': path,
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTreePartialViewItem({
        path,
    }: {
        path?: Array<string>,
    }): CancelablePromise<Array<FileSystemTreeItemModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/partial-view/item',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns PagedFileSystemTreeItemModel Success
     * @throws ApiError
     */
    public static getTreePartialViewRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedFileSystemTreeItemModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/partial-view/root',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
