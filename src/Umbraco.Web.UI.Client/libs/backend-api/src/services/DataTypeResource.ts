/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DataTypeCopyModel } from '../models/DataTypeCopyModel';
import type { DataTypeCreateModel } from '../models/DataTypeCreateModel';
import type { DataTypeModel } from '../models/DataTypeModel';
import type { DataTypeMoveModel } from '../models/DataTypeMoveModel';
import type { DataTypeReferenceModel } from '../models/DataTypeReferenceModel';
import type { DataTypeUpdateModel } from '../models/DataTypeUpdateModel';
import type { DocumentTypeTreeItemModel } from '../models/DocumentTypeTreeItemModel';
import type { FolderCreateModel } from '../models/FolderCreateModel';
import type { FolderModel } from '../models/FolderModel';
import type { FolderTreeItemModel } from '../models/FolderTreeItemModel';
import type { FolderUpdateModel } from '../models/FolderUpdateModel';
import type { PagedFolderTreeItemModel } from '../models/PagedFolderTreeItemModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DataTypeResource {

    /**
     * @returns any Created
     * @throws ApiError
     */
    public static postDataType({
requestBody,
}: {
requestBody?: DataTypeCreateModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/data-type',
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
    public static getDataTypeByKey({
        key,
    }: {
        key: string,
    }): CancelablePromise<DataTypeModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/data-type/{key}',
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
    public static deleteDataTypeByKey({
key,
}: {
key: string,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/data-type/{key}',
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
    public static putDataTypeByKey({
key,
requestBody,
}: {
key: string,
requestBody?: DataTypeUpdateModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/data-type/{key}',
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
    public static postDataTypeByKeyCopy({
        key,
        requestBody,
    }: {
        key: string,
        requestBody?: DataTypeCopyModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/data-type/{key}/copy',
            path: {
                'key': key,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postDataTypeByKeyMove({
        key,
        requestBody,
    }: {
        key: string,
        requestBody?: DataTypeMoveModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/data-type/{key}/move',
            path: {
                'key': key,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDataTypeByKeyReferences({
        key,
    }: {
        key: string,
    }): CancelablePromise<Array<DataTypeReferenceModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/data-type/{key}/references',
            path: {
                'key': key,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Created
     * @throws ApiError
     */
    public static postDataTypeFolder({
requestBody,
}: {
requestBody?: FolderCreateModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/data-type/folder',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDataTypeFolderByKey({
        key,
    }: {
        key: string,
    }): CancelablePromise<FolderModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/data-type/folder/{key}',
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
    public static deleteDataTypeFolderByKey({
key,
}: {
key: string,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/data-type/folder/{key}',
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
    public static putDataTypeFolderByKey({
key,
requestBody,
}: {
key: string,
requestBody?: FolderUpdateModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/data-type/folder/{key}',
            path: {
                'key': key,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns PagedFolderTreeItemModel Success
     * @throws ApiError
     */
    public static getTreeDataTypeChildren({
        parentKey,
        skip,
        take = 100,
        foldersOnly = false,
    }: {
        parentKey?: string,
        skip?: number,
        take?: number,
        foldersOnly?: boolean,
    }): CancelablePromise<PagedFolderTreeItemModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/data-type/children',
            query: {
                'parentKey': parentKey,
                'skip': skip,
                'take': take,
                'foldersOnly': foldersOnly,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTreeDataTypeItem({
        key,
    }: {
        key?: Array<string>,
    }): CancelablePromise<Array<(FolderTreeItemModel | DocumentTypeTreeItemModel)>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/data-type/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedFolderTreeItemModel Success
     * @throws ApiError
     */
    public static getTreeDataTypeRoot({
        skip,
        take = 100,
        foldersOnly = false,
    }: {
        skip?: number,
        take?: number,
        foldersOnly?: boolean,
    }): CancelablePromise<PagedFolderTreeItemModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/data-type/root',
            query: {
                'skip': skip,
                'take': take,
                'foldersOnly': foldersOnly,
            },
        });
    }

}
