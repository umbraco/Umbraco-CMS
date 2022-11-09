/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { FileSystemTreeItem } from '../models/FileSystemTreeItem';
import type { PagedFileSystemTreeItem } from '../models/PagedFileSystemTreeItem';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ScriptResource {

    /**
     * @returns PagedFileSystemTreeItem Success
     * @throws ApiError
     */
    public static getUmbracoManagementApiV1TreeScriptChildren({
        path,
        skip,
        take = 100,
    }: {
        path?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedFileSystemTreeItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/script/children',
            query: {
                'path': path,
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns FileSystemTreeItem Success
     * @throws ApiError
     */
    public static getUmbracoManagementApiV1TreeScriptItem({
        path,
    }: {
        path?: Array<string>,
    }): CancelablePromise<Array<FileSystemTreeItem>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/script/item',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns PagedFileSystemTreeItem Success
     * @throws ApiError
     */
    public static getUmbracoManagementApiV1TreeScriptRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedFileSystemTreeItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/script/root',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
