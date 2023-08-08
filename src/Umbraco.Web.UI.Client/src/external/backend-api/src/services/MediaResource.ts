/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateMediaRequestModel } from '../models/CreateMediaRequestModel';
import type { DocumentResponseModel } from '../models/DocumentResponseModel';
import type { MediaItemResponseModel } from '../models/MediaItemResponseModel';
import type { MediaTreeItemResponseModel } from '../models/MediaTreeItemResponseModel';
import type { MoveMediaRequestModel } from '../models/MoveMediaRequestModel';
import type { PagedMediaTreeItemResponseModel } from '../models/PagedMediaTreeItemResponseModel';
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
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getMediaById({
        id,
    }: {
        id: string,
    }): CancelablePromise<DocumentResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/media/{id}',
            path: {
                'id': id,
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
    public static deleteMediaById({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/media/{id}',
            path: {
                'id': id,
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
    public static putMediaById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateMediaRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/media/{id}',
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
     * @returns any Success
     * @throws ApiError
     */
    public static putMediaByIdMove({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: MoveMediaRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/media/{id}/move',
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
     * @returns any Success
     * @throws ApiError
     */
    public static getMediaItem({
        id,
        dataTypeId,
    }: {
        id?: Array<string>,
        dataTypeId?: string,
    }): CancelablePromise<Array<MediaItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/media/item',
            query: {
                'id': id,
                'dataTypeId': dataTypeId,
            },
        });
    }

    /**
     * @returns PagedRecycleBinItemResponseModel Success
     * @throws ApiError
     */
    public static getRecycleBinMediaChildren({
        parentId,
        skip,
        take = 100,
    }: {
        parentId?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedRecycleBinItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/recycle-bin/media/children',
            query: {
                'parentId': parentId,
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
     * @returns PagedMediaTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeMediaChildren({
        parentId,
        skip,
        take = 100,
        dataTypeId,
    }: {
        parentId?: string,
        skip?: number,
        take?: number,
        dataTypeId?: string,
    }): CancelablePromise<PagedMediaTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media/children',
            query: {
                'parentId': parentId,
                'skip': skip,
                'take': take,
                'dataTypeId': dataTypeId,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTreeMediaItem({
        id,
        dataTypeId,
    }: {
        id?: Array<string>,
        dataTypeId?: string,
    }): CancelablePromise<Array<MediaTreeItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media/item',
            query: {
                'id': id,
                'dataTypeId': dataTypeId,
            },
        });
    }

    /**
     * @returns PagedMediaTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeMediaRoot({
        skip,
        take = 100,
        dataTypeId,
    }: {
        skip?: number,
        take?: number,
        dataTypeId?: string,
    }): CancelablePromise<PagedMediaTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media/root',
            query: {
                'skip': skip,
                'take': take,
                'dataTypeId': dataTypeId,
            },
        });
    }

}
