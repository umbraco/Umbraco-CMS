/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateDictionaryItemRequestModel } from '../models/CreateDictionaryItemRequestModel';
import type { DictionaryItemItemResponseModel } from '../models/DictionaryItemItemResponseModel';
import type { DictionaryItemResponseModel } from '../models/DictionaryItemResponseModel';
import type { ImportDictionaryRequestModel } from '../models/ImportDictionaryRequestModel';
import type { MoveDictionaryRequestModel } from '../models/MoveDictionaryRequestModel';
import type { PagedDictionaryOverviewResponseModel } from '../models/PagedDictionaryOverviewResponseModel';
import type { PagedNamedEntityTreeItemResponseModel } from '../models/PagedNamedEntityTreeItemResponseModel';
import type { UpdateDictionaryItemRequestModel } from '../models/UpdateDictionaryItemRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DictionaryResource {

    /**
     * @returns PagedDictionaryOverviewResponseModel Success
     * @throws ApiError
     */
    public static getDictionary({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedDictionaryOverviewResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/dictionary',
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
     * @returns string Created
     * @throws ApiError
     */
    public static postDictionary({
        requestBody,
    }: {
        requestBody?: CreateDictionaryItemRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/dictionary',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Generated-Resource',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
                409: `Conflict`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDictionaryById({
        id,
    }: {
        id: string,
    }): CancelablePromise<DictionaryItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/dictionary/{id}',
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
     * @returns any Success
     * @throws ApiError
     */
    public static deleteDictionaryById({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/dictionary/{id}',
            path: {
                'id': id,
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
    public static putDictionaryById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateDictionaryItemRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/dictionary/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns binary Success
     * @throws ApiError
     */
    public static getDictionaryByIdExport({
        id,
        includeChildren = false,
    }: {
        id: string,
        includeChildren?: boolean,
    }): CancelablePromise<Blob> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/dictionary/{id}/export',
            path: {
                'id': id,
            },
            query: {
                'includeChildren': includeChildren,
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
    public static putDictionaryByIdMove({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: MoveDictionaryRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/dictionary/{id}/move',
            path: {
                'id': id,
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
    public static postDictionaryImport({
        requestBody,
    }: {
        requestBody?: ImportDictionaryRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/dictionary/import',
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
    public static getItemDictionary({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<DictionaryItemItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/item/dictionary',
            query: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns PagedNamedEntityTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeDictionaryChildren({
        parentId,
        skip,
        take = 100,
    }: {
        parentId?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedNamedEntityTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/dictionary/children',
            query: {
                'parentId': parentId,
                'skip': skip,
                'take': take,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns PagedNamedEntityTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeDictionaryRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedNamedEntityTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/dictionary/root',
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
