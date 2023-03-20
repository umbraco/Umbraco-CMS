/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ContentTreeItemResponseModel } from '../models/ContentTreeItemResponseModel';
import type { CreateMediaRequestModel } from '../models/CreateMediaRequestModel';
import type { DocumentResponseModel } from '../models/DocumentResponseModel';
import type { DocumentTreeItemResponseModel } from '../models/DocumentTreeItemResponseModel';
import type { PagedContentTreeItemResponseModel } from '../models/PagedContentTreeItemResponseModel';
import type { PagedRecycleBinItemResponseModel } from '../models/PagedRecycleBinItemResponseModel';
import type { UpdateMediaRequestModel } from '../models/UpdateMediaRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MediaResource {

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postMedia({
requestBody,
}: {
requestBody?: CreateMediaRequestModel,
}): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/media',
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
    public static getMediaByKey({
key,
}: {
key: string,
}): CancelablePromise<DocumentResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/media/{key}',
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
    public static deleteMediaByKey({
key,
}: {
key: string,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/media/{key}',
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
    public static putMediaByKey({
key,
requestBody,
}: {
key: string,
requestBody?: UpdateMediaRequestModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/media/{key}',
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
     * @returns PagedRecycleBinItemResponseModel Success
     * @throws ApiError
     */
    public static getRecycleBinMediaChildren({
parentKey,
skip,
take = 100,
}: {
parentKey?: string,
skip?: number,
take?: number,
}): CancelablePromise<PagedRecycleBinItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/recycle-bin/media/children',
            query: {
                'parentKey': parentKey,
                'skip': skip,
                'take': take,
            },
            errors: {
                401: `Unauthorized`,
            },
        });
    }

    /**
     * @returns PagedRecycleBinItemResponseModel Success
     * @throws ApiError
     */
    public static getRecycleBinMediaRoot({
skip,
take = 100,
}: {
skip?: number,
take?: number,
}): CancelablePromise<PagedRecycleBinItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/recycle-bin/media/root',
            query: {
                'skip': skip,
                'take': take,
            },
            errors: {
                401: `Unauthorized`,
            },
        });
    }

    /**
     * @returns PagedContentTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeMediaChildren({
parentKey,
skip,
take = 100,
dataTypeKey,
}: {
parentKey?: string,
skip?: number,
take?: number,
dataTypeKey?: string,
}): CancelablePromise<PagedContentTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media/children',
            query: {
                'parentKey': parentKey,
                'skip': skip,
                'take': take,
                'dataTypeKey': dataTypeKey,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTreeMediaItem({
key,
dataTypeKey,
}: {
key?: Array<string>,
dataTypeKey?: string,
}): CancelablePromise<Array<(ContentTreeItemResponseModel | DocumentTreeItemResponseModel)>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media/item',
            query: {
                'key': key,
                'dataTypeKey': dataTypeKey,
            },
        });
    }

    /**
     * @returns PagedContentTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeMediaRoot({
skip,
take = 100,
dataTypeKey,
}: {
skip?: number,
take?: number,
dataTypeKey?: string,
}): CancelablePromise<PagedContentTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media/root',
            query: {
                'skip': skip,
                'take': take,
                'dataTypeKey': dataTypeKey,
            },
        });
    }

}
