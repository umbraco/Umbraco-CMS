/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateStylesheetFolderRequestModel } from '../models/CreateStylesheetFolderRequestModel';
import type { CreateStylesheetRequestModel } from '../models/CreateStylesheetRequestModel';
import type { PagedFileSystemTreeItemPresentationModel } from '../models/PagedFileSystemTreeItemPresentationModel';
import type { RenameStylesheetRequestModel } from '../models/RenameStylesheetRequestModel';
import type { StylesheetFolderResponseModel } from '../models/StylesheetFolderResponseModel';
import type { StylesheetItemResponseModel } from '../models/StylesheetItemResponseModel';
import type { StylesheetResponseModel } from '../models/StylesheetResponseModel';
import type { UpdateStylesheetRequestModel } from '../models/UpdateStylesheetRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class StylesheetResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getItemStylesheet({
        path,
    }: {
        path?: Array<string>,
    }): CancelablePromise<Array<StylesheetItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/item/stylesheet',
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
    public static postStylesheet({
        requestBody,
    }: {
        requestBody?: CreateStylesheetRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/stylesheet',
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
    public static getStylesheetByPath({
        path,
    }: {
        path: string,
    }): CancelablePromise<StylesheetResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/stylesheet/{path}',
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
     * @returns any Success
     * @throws ApiError
     */
    public static deleteStylesheetByPath({
        path,
    }: {
        path: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/stylesheet/{path}',
            path: {
                'path': path,
            },
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
    public static putStylesheetByPath({
        path,
        requestBody,
    }: {
        path: string,
        requestBody?: UpdateStylesheetRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/stylesheet/{path}',
            path: {
                'path': path,
            },
            body: requestBody,
            mediaType: 'application/json',
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
    public static putStylesheetByPathRename({
        path,
        requestBody,
    }: {
        path: string,
        requestBody?: RenameStylesheetRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/stylesheet/{path}/rename',
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
    public static postStylesheetFolder({
        requestBody,
    }: {
        requestBody?: CreateStylesheetFolderRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/stylesheet/folder',
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
    public static getStylesheetFolderByPath({
        path,
    }: {
        path: string,
    }): CancelablePromise<StylesheetFolderResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/stylesheet/folder/{path}',
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
     * @returns any Success
     * @throws ApiError
     */
    public static deleteStylesheetFolderByPath({
        path,
    }: {
        path: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/stylesheet/folder/{path}',
            path: {
                'path': path,
            },
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns PagedFileSystemTreeItemPresentationModel Success
     * @throws ApiError
     */
    public static getTreeStylesheetChildren({
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
            url: '/umbraco/management/api/v1/tree/stylesheet/children',
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
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

}
