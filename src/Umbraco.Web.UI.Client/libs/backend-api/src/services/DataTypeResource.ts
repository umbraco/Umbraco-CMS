/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CopyDataTypeRequestModel } from '../models/CopyDataTypeRequestModel';
import type { CreateDataTypeRequestModel } from '../models/CreateDataTypeRequestModel';
import type { CreateFolderRequestModel } from '../models/CreateFolderRequestModel';
import type { DataTypeReferenceResponseModel } from '../models/DataTypeReferenceResponseModel';
import type { DataTypeResponseModel } from '../models/DataTypeResponseModel';
import type { DocumentTypeTreeItemResponseModel } from '../models/DocumentTypeTreeItemResponseModel';
import type { FolderReponseModel } from '../models/FolderReponseModel';
import type { FolderTreeItemResponseModel } from '../models/FolderTreeItemResponseModel';
import type { MoveDataTypeRequestModel } from '../models/MoveDataTypeRequestModel';
import type { PagedFolderTreeItemResponseModel } from '../models/PagedFolderTreeItemResponseModel';
import type { UpdateDataTypeRequestModel } from '../models/UpdateDataTypeRequestModel';
import type { UpdateFolderReponseModel } from '../models/UpdateFolderReponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DataTypeResource {

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postDataType({
requestBody,
}: {
requestBody?: CreateDataTypeRequestModel,
}): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/data-type',
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
    public static getDataTypeByKey({
key,
}: {
key: string,
}): CancelablePromise<DataTypeResponseModel> {
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
requestBody?: UpdateDataTypeRequestModel,
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
     * @returns string Created
     * @throws ApiError
     */
    public static postDataTypeByKeyCopy({
key,
requestBody,
}: {
key: string,
requestBody?: CopyDataTypeRequestModel,
}): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/data-type/{key}/copy',
            path: {
                'key': key,
            },
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Location',
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
requestBody?: MoveDataTypeRequestModel,
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
}): CancelablePromise<Array<DataTypeReferenceResponseModel>> {
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
     * @returns string Created
     * @throws ApiError
     */
    public static postDataTypeFolder({
requestBody,
}: {
requestBody?: CreateFolderRequestModel,
}): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/data-type/folder',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Location',
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
}): CancelablePromise<FolderReponseModel> {
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
requestBody?: UpdateFolderReponseModel,
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
     * @returns PagedFolderTreeItemResponseModel Success
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
}): CancelablePromise<PagedFolderTreeItemResponseModel> {
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
}): CancelablePromise<Array<(FolderTreeItemResponseModel | DocumentTypeTreeItemResponseModel)>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/data-type/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedFolderTreeItemResponseModel Success
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
}): CancelablePromise<PagedFolderTreeItemResponseModel> {
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
