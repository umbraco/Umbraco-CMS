/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateDictionaryItemRequestModel } from '../models/CreateDictionaryItemRequestModel';
import type { DictionaryItemResponseModel } from '../models/DictionaryItemResponseModel';
import type { DocumentTypeTreeItemResponseModel } from '../models/DocumentTypeTreeItemResponseModel';
import type { FolderTreeItemResponseModel } from '../models/FolderTreeItemResponseModel';
import type { ImportDictionaryRequestModel } from '../models/ImportDictionaryRequestModel';
import type { MoveDictionaryRequestModel } from '../models/MoveDictionaryRequestModel';
import type { PagedDictionaryOverviewResponseModel } from '../models/PagedDictionaryOverviewResponseModel';
import type { PagedEntityTreeItemResponseModel } from '../models/PagedEntityTreeItemResponseModel';
import type { UpdateDictionaryItemRequestModel } from '../models/UpdateDictionaryItemRequestModel';
import type { UploadDictionaryResponseModel } from '../models/UploadDictionaryResponseModel';

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
            responseHeader: 'Location',
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
}): CancelablePromise<DictionaryItemResponseModel> {
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
requestBody?: UpdateDictionaryItemRequestModel,
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
requestBody?: MoveDictionaryRequestModel,
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
            responseHeader: 'Location',
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
}): CancelablePromise<UploadDictionaryResponseModel> {
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
     * @returns PagedEntityTreeItemResponseModel Success
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
}): CancelablePromise<PagedEntityTreeItemResponseModel> {
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
}): CancelablePromise<Array<(FolderTreeItemResponseModel | DocumentTypeTreeItemResponseModel)>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/dictionary/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeDictionaryRoot({
skip,
take = 100,
}: {
skip?: number,
take?: number,
}): CancelablePromise<PagedEntityTreeItemResponseModel> {
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
