/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DataType } from '../models/DataType';
import type { DataTypeCreateModel } from '../models/DataTypeCreateModel';
import type { DataTypeReference } from '../models/DataTypeReference';
import type { DataTypeUpdateModel } from '../models/DataTypeUpdateModel';
import type { Folder } from '../models/Folder';
import type { FolderCreateModel } from '../models/FolderCreateModel';
import type { FolderTreeItem } from '../models/FolderTreeItem';
import type { FolderUpdateModel } from '../models/FolderUpdateModel';
import type { PagedFolderTreeItem } from '../models/PagedFolderTreeItem';

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
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns DataType Success
     * @throws ApiError
     */
    public static getDataTypeByKey({
        key,
    }: {
        key: string,
    }): CancelablePromise<DataType> {
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
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns DataTypeReference Success
     * @throws ApiError
     */
    public static getDataTypeByKeyReferences({
        key,
    }: {
        key: string,
    }): CancelablePromise<Array<DataTypeReference>> {
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
     * @returns Folder Success
     * @throws ApiError
     */
    public static getDataTypeFolderByKey({
        key,
    }: {
        key: string,
    }): CancelablePromise<Folder> {
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
     * @returns PagedFolderTreeItem Success
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
    }): CancelablePromise<PagedFolderTreeItem> {
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
     * @returns FolderTreeItem Success
     * @throws ApiError
     */
    public static getTreeDataTypeItem({
        key,
    }: {
        key?: Array<string>,
    }): CancelablePromise<Array<FolderTreeItem>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/data-type/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedFolderTreeItem Success
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
    }): CancelablePromise<PagedFolderTreeItem> {
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
