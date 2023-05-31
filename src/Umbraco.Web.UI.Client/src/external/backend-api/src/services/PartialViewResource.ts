/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreatePartialViewRequestModel } from '../models/CreatePartialViewRequestModel';
import type { CreatePathFolderRequestModel } from '../models/CreatePathFolderRequestModel';
import type { PagedFileSystemTreeItemPresentationModel } from '../models/PagedFileSystemTreeItemPresentationModel';
import type { PagedSnippetItemResponseModel } from '../models/PagedSnippetItemResponseModel';
import type { PartialViewItemResponseModel } from '../models/PartialViewItemResponseModel';
import type { PartialViewResponseModel } from '../models/PartialViewResponseModel';
import type { PartialViewSnippetResponseModel } from '../models/PartialViewSnippetResponseModel';
import type { UpdatePartialViewRequestModel } from '../models/UpdatePartialViewRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class PartialViewResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getPartialView({
        path,
    }: {
        path?: string,
    }): CancelablePromise<PartialViewResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/partial-view',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postPartialView({
        requestBody,
    }: {
        requestBody?: CreatePartialViewRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/partial-view',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Location',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deletePartialView({
        path,
    }: {
        path?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/partial-view',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putPartialView({
        requestBody,
    }: {
        requestBody?: UpdatePartialViewRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/partial-view',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getPartialViewFolder({
        path,
    }: {
        path?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/partial-view/folder',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postPartialViewFolder({
        requestBody,
    }: {
        requestBody?: CreatePathFolderRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/partial-view/folder',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deletePartialViewFolder({
        path,
    }: {
        path?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/partial-view/folder',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getPartialViewItem({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<PartialViewItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/partial-view/item',
            query: {
                'id': id,
            },
        });
    }

    /**
     * @returns PagedSnippetItemResponseModel Success
     * @throws ApiError
     */
    public static getPartialViewSnippet({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedSnippetItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/partial-view/snippet',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getPartialViewSnippetByName({
        name,
    }: {
        name: string,
    }): CancelablePromise<PartialViewSnippetResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/partial-view/snippet/{name}',
            path: {
                'name': name,
            },
        });
    }

    /**
     * @returns PagedFileSystemTreeItemPresentationModel Success
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
    }): CancelablePromise<PagedFileSystemTreeItemPresentationModel> {
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
     * @returns PagedFileSystemTreeItemPresentationModel Success
     * @throws ApiError
     */
    public static getTreePartialViewRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedFileSystemTreeItemPresentationModel> {
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
