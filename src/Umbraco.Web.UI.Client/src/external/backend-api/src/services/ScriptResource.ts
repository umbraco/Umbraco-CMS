/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateScriptFolderRequestModel } from '../models/CreateScriptFolderRequestModel';
import type { CreateScriptRequestModel } from '../models/CreateScriptRequestModel';
import type { PagedFileSystemTreeItemPresentationModel } from '../models/PagedFileSystemTreeItemPresentationModel';
import type { RenameScriptRequestModel } from '../models/RenameScriptRequestModel';
import type { ScriptFolderResponseModel } from '../models/ScriptFolderResponseModel';
import type { ScriptItemResponseModel } from '../models/ScriptItemResponseModel';
import type { ScriptResponseModel } from '../models/ScriptResponseModel';
import type { UpdateScriptRequestModel } from '../models/UpdateScriptRequestModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class ScriptResource {
    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getItemScript({
        path,
    }: {
        path?: Array<string>,
    }): CancelablePromise<Array<ScriptItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/item/script',
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
    public static postScript({
        requestBody,
    }: {
        requestBody?: CreateScriptRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/script',
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
    public static getScriptByPath({
        path,
    }: {
        path: string,
    }): CancelablePromise<ScriptResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/script/{path}',
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
    public static deleteScriptByPath({
        path,
    }: {
        path: string,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/script/{path}',
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
    public static putScriptByPath({
        path,
        requestBody,
    }: {
        path: string,
        requestBody?: UpdateScriptRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/script/{path}',
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
    public static putScriptByPathRename({
        path,
        requestBody,
    }: {
        path: string,
        requestBody?: RenameScriptRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/script/{path}/rename',
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
    public static postScriptFolder({
        requestBody,
    }: {
        requestBody?: CreateScriptFolderRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/script/folder',
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
    public static getScriptFolderByPath({
        path,
    }: {
        path: string,
    }): CancelablePromise<ScriptFolderResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/script/folder/{path}',
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
    public static deleteScriptFolderByPath({
        path,
    }: {
        path: string,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/script/folder/{path}',
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
     * @returns PagedFileSystemTreeItemPresentationModel Success
     * @throws ApiError
     */
    public static getTreeScriptChildren({
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
            url: '/umbraco/management/api/v1/tree/script/children',
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
    public static getTreeScriptRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedFileSystemTreeItemPresentationModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/script/root',
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
