/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { FileSystemTreeItemPresentationModel } from '../models/FileSystemTreeItemPresentationModel';
import type { PagedFileSystemTreeItemPresentationModel } from '../models/PagedFileSystemTreeItemPresentationModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class StylesheetResource {

    /**
     * @returns PagedFileSystemTreeItemPresentationModel Success
     * @throws ApiError
     */
    public static getTreeStylesheetChildren({
path,
skip,
take = 100,
}: {
path?: string,
skip?: number,
take?: number,
}): CancelablePromise<PagedFileSystemTreeItemPresentationModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/stylesheet/children',
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
    public static getTreeStylesheetItem({
path,
}: {
path?: Array<string>,
}): CancelablePromise<Array<FileSystemTreeItemPresentationModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/stylesheet/item',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns PagedFileSystemTreeItemPresentationModel Success
     * @throws ApiError
     */
    public static getTreeStylesheetRoot({
skip,
take = 100,
}: {
skip?: number,
take?: number,
}): CancelablePromise<PagedFileSystemTreeItemPresentationModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/stylesheet/root',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
