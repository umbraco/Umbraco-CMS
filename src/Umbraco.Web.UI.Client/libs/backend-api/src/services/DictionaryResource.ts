/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ContentResult } from '../models/ContentResult';
import type { CreatedResult } from '../models/CreatedResult';
import type { Dictionary } from '../models/Dictionary';
import type { DictionaryImport } from '../models/DictionaryImport';
import type { DictionaryItem } from '../models/DictionaryItem';
import type { FolderTreeItem } from '../models/FolderTreeItem';
import type { JsonPatch } from '../models/JsonPatch';
import type { PagedDictionaryOverview } from '../models/PagedDictionaryOverview';
import type { PagedEntityTreeItem } from '../models/PagedEntityTreeItem';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DictionaryResource {

    /**
     * @returns PagedDictionaryOverview Success
     * @throws ApiError
     */
    public static getDictionary({
        skip,
        take,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedDictionaryOverview> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/dictionary',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns CreatedResult Created
     * @throws ApiError
     */
    public static postDictionary({
        requestBody,
    }: {
        requestBody?: DictionaryItem,
    }): CancelablePromise<CreatedResult> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/dictionary',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns ContentResult Success
     * @throws ApiError
     */
    public static patchDictionaryById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: Array<JsonPatch>,
    }): CancelablePromise<ContentResult> {
        return __request(OpenAPI, {
            method: 'PATCH',
            url: '/umbraco/management/api/v1/dictionary/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns Dictionary Success
     * @throws ApiError
     */
    public static getDictionaryByKey({
        key,
    }: {
        key: string,
    }): CancelablePromise<Dictionary> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/dictionary/{key}',
            path: {
                'key': key,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteDictionaryByKey({
        key,
    }: {
        key: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/dictionary/{key}',
            path: {
                'key': key,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns binary Success
     * @throws ApiError
     */
    public static getDictionaryExportByKey({
        key,
        includeChildren = false,
    }: {
        key: string,
        includeChildren?: boolean,
    }): CancelablePromise<Blob> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/dictionary/export/{key}',
            path: {
                'key': key,
            },
            query: {
                'includeChildren': includeChildren,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns ContentResult Success
     * @throws ApiError
     */
    public static postDictionaryImport({
        file,
        parentId,
    }: {
        file?: string,
        parentId?: number,
    }): CancelablePromise<ContentResult> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/dictionary/import',
            query: {
                'file': file,
                'parentId': parentId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns DictionaryImport Success
     * @throws ApiError
     */
    public static postDictionaryUpload({
        requestBody,
    }: {
        requestBody?: any,
    }): CancelablePromise<DictionaryImport> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/dictionary/upload',
            body: requestBody,
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItem Success
     * @throws ApiError
     */
    public static getTreeDictionaryChildren({
        parentKey,
        skip,
        take = 100,
    }: {
        parentKey?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedEntityTreeItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/dictionary/children',
            query: {
                'parentKey': parentKey,
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns FolderTreeItem Success
     * @throws ApiError
     */
    public static getTreeDictionaryItem({
        key,
    }: {
        key?: Array<string>,
    }): CancelablePromise<Array<FolderTreeItem>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/dictionary/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItem Success
     * @throws ApiError
     */
    public static getTreeDictionaryRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedEntityTreeItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/dictionary/root',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
