/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedFileSystemTreeItemPresentationModel } from '../models/PagedFileSystemTreeItemPresentationModel';
import type { StaticFileItemResponseModel } from '../models/StaticFileItemResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class StaticFileResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getItemStaticFile({
        path,
    }: {
        path?: Array<string>,
    }): CancelablePromise<Array<StaticFileItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/item/static-file',
            query: {
                'path': path,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns PagedFileSystemTreeItemPresentationModel Success
     * @throws ApiError
     */
    public static getTreeStaticFileChildren({
        parentPath,
        skip,
        take = 100,
    }: {
        parentPath?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedFileSystemTreeItemPresentationModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/static-file/children',
            query: {
                'parentPath': parentPath,
                'skip': skip,
                'take': take,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns PagedFileSystemTreeItemPresentationModel Success
     * @throws ApiError
     */
    public static getTreeStaticFileRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedFileSystemTreeItemPresentationModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/static-file/root',
            query: {
                'skip': skip,
                'take': take,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

}
