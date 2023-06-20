/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreatePathFolderRequestModel } from '../models/CreatePathFolderRequestModel';
import type { CreateScriptRequestModel } from '../models/CreateScriptRequestModel';
import type { PagedFileSystemTreeItemPresentationModel } from '../models/PagedFileSystemTreeItemPresentationModel';
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
    public static getScript({
        path,
    }: {
        path?: string,
    }): CancelablePromise<ScriptResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/script',
            query: {
                'path': path,
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
            responseHeader: 'Location',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteScript({
        path,
    }: {
        path?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/script',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putScript({
        requestBody,
    }: {
        requestBody?: UpdateScriptRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/script',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getScriptFolder({
        path,
    }: {
        path?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/script/folder',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postScriptFolder({
        requestBody,
    }: {
        requestBody?: CreatePathFolderRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/script/folder',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteScriptFolder({
        path,
    }: {
        path?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/script/folder',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getScriptItem({
        path,
    }: {
        path?: Array<string>,
    }): CancelablePromise<Array<ScriptItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/script/item',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns PagedFileSystemTreeItemPresentationModel Success
     * @throws ApiError
     */
    public static getTreeScriptChildren({
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
            url: '/umbraco/management/api/v1/tree/script/children',
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
        });
    }

}
