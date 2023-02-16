/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DictionaryImportModel } from '../models/DictionaryImportModel';
import type { DictionaryItemCreateModel } from '../models/DictionaryItemCreateModel';
import type { DictionaryItemModel } from '../models/DictionaryItemModel';
import type { DictionaryItemUpdateModel } from '../models/DictionaryItemUpdateModel';
import type { DictionaryMoveModel } from '../models/DictionaryMoveModel';
import type { DictionaryUploadModel } from '../models/DictionaryUploadModel';
import type { DocumentTypeTreeItemModel } from '../models/DocumentTypeTreeItemModel';
import type { FolderTreeItemModel } from '../models/FolderTreeItemModel';
import type { PagedDictionaryOverviewModel } from '../models/PagedDictionaryOverviewModel';
import type { PagedEntityTreeItemModel } from '../models/PagedEntityTreeItemModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DictionaryResource {

    /**
     * @returns PagedDictionaryOverviewModel Success
     * @throws ApiError
     */
    public static getDictionary({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedDictionaryOverviewModel> {
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
     * @returns any Created
     * @throws ApiError
     */
    public static postDictionary({
requestBody,
}: {
requestBody?: DictionaryItemCreateModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/dictionary',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                404: `Not Found`,
                409: `Conflict`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDictionaryByKey({
        key,
    }: {
        key: string,
    }): CancelablePromise<DictionaryItemModel> {
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
                400: `Bad Request`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putDictionaryByKey({
key,
requestBody,
}: {
key: string,
requestBody?: DictionaryItemUpdateModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/dictionary/{key}',
            path: {
                'key': key,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns binary Success
     * @throws ApiError
     */
    public static getDictionaryByKeyExport({
        key,
        includeChildren = false,
    }: {
        key: string,
        includeChildren?: boolean,
    }): CancelablePromise<Blob> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/dictionary/{key}/export',
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
     * @returns any Success
     * @throws ApiError
     */
    public static postDictionaryByKeyMove({
        key,
        requestBody,
    }: {
        key: string,
        requestBody?: DictionaryMoveModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/dictionary/{key}/move',
            path: {
                'key': key,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Created
     * @throws ApiError
     */
    public static postDictionaryImport({
        requestBody,
    }: {
        requestBody?: DictionaryImportModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/dictionary/import',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postDictionaryUpload({
        requestBody,
    }: {
        requestBody?: any,
    }): CancelablePromise<DictionaryUploadModel> {
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
     * @returns PagedEntityTreeItemModel Success
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
    }): CancelablePromise<PagedEntityTreeItemModel> {
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
     * @returns any Success
     * @throws ApiError
     */
    public static getTreeDictionaryItem({
        key,
    }: {
        key?: Array<string>,
    }): CancelablePromise<Array<(FolderTreeItemModel | DocumentTypeTreeItemModel)>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/dictionary/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItemModel Success
     * @throws ApiError
     */
    public static getTreeDictionaryRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedEntityTreeItemModel> {
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
