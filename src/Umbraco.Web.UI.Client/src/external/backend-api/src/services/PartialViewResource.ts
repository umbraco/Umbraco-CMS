/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreatePartialViewFolderRequestModel } from '../models/CreatePartialViewFolderRequestModel';
import type { CreatePartialViewRequestModel } from '../models/CreatePartialViewRequestModel';
import type { PagedFileSystemTreeItemPresentationModel } from '../models/PagedFileSystemTreeItemPresentationModel';
import type { PagedPartialViewSnippetItemResponseModel } from '../models/PagedPartialViewSnippetItemResponseModel';
import type { PartialViewFolderResponseModel } from '../models/PartialViewFolderResponseModel';
import type { PartialViewItemResponseModel } from '../models/PartialViewItemResponseModel';
import type { PartialViewResponseModel } from '../models/PartialViewResponseModel';
import type { PartialViewSnippetResponseModel } from '../models/PartialViewSnippetResponseModel';
import type { RenamePartialViewRequestModel } from '../models/RenamePartialViewRequestModel';
import type { UpdatePartialViewRequestModel } from '../models/UpdatePartialViewRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class PartialViewResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getItemPartialView({
        path,
    }: {
        path?: Array<string>,
    }): CancelablePromise<Array<PartialViewItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/item/partial-view',
            query: {
                'path': path,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
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
            responseHeader: 'Umb-Generated-Resource',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getPartialViewByPath({
        path,
    }: {
        path: string,
    }): CancelablePromise<PartialViewResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/partial-view/{path}',
            path: {
                'path': path,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static deletePartialViewByPath({
        path,
    }: {
        path: string,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/partial-view/{path}',
            path: {
                'path': path,
            },
            responseHeader: 'Umb-Notifications',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static putPartialViewByPath({
        path,
        requestBody,
    }: {
        path: string,
        requestBody?: UpdatePartialViewRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/partial-view/{path}',
            path: {
                'path': path,
            },
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static putPartialViewByPathRename({
        path,
        requestBody,
    }: {
        path: string,
        requestBody?: RenamePartialViewRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/partial-view/{path}/rename',
            path: {
                'path': path,
            },
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Generated-Resource',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postPartialViewFolder({
        requestBody,
    }: {
        requestBody?: CreatePartialViewFolderRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/partial-view/folder',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Generated-Resource',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getPartialViewFolderByPath({
        path,
    }: {
        path: string,
    }): CancelablePromise<PartialViewFolderResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/partial-view/folder/{path}',
            path: {
                'path': path,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static deletePartialViewFolderByPath({
        path,
    }: {
        path: string,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/partial-view/folder/{path}',
            path: {
                'path': path,
            },
            responseHeader: 'Umb-Notifications',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns PagedPartialViewSnippetItemResponseModel Success
     * @throws ApiError
     */
    public static getPartialViewSnippet({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedPartialViewSnippetItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/partial-view/snippet',
            query: {
                'skip': skip,
                'take': take,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getPartialViewSnippetById({
        id,
    }: {
        id: string,
    }): CancelablePromise<PartialViewSnippetResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/partial-view/snippet/{id}',
            path: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns PagedFileSystemTreeItemPresentationModel Success
     * @throws ApiError
     */
    public static getTreePartialViewChildren({
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
            url: '/umbraco/management/api/v1/tree/partial-view/children',
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
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

}
