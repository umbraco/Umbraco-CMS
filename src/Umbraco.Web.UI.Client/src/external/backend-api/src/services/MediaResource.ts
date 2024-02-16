/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateMediaRequestModel } from '../models/CreateMediaRequestModel';
import type { DirectionModel } from '../models/DirectionModel';
import type { MediaConfigurationResponseModel } from '../models/MediaConfigurationResponseModel';
import type { MediaItemResponseModel } from '../models/MediaItemResponseModel';
import type { MediaResponseModel } from '../models/MediaResponseModel';
import type { MoveMediaRequestModel } from '../models/MoveMediaRequestModel';
import type { PagedMediaCollectionResponseModel } from '../models/PagedMediaCollectionResponseModel';
import type { PagedMediaRecycleBinItemResponseModel } from '../models/PagedMediaRecycleBinItemResponseModel';
import type { PagedMediaTreeItemResponseModel } from '../models/PagedMediaTreeItemResponseModel';
import type { SortingRequestModel } from '../models/SortingRequestModel';
import type { UpdateMediaRequestModel } from '../models/UpdateMediaRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MediaResource {

    /**
     * @returns PagedMediaCollectionResponseModel Success
     * @throws ApiError
     */
    public static getCollectionMedia({
id,
dataTypeId,
orderBy = 'updateDate',
orderDirection,
filter,
skip,
take = 100,
}: {
id?: string,
dataTypeId?: string,
orderBy?: string,
orderDirection?: DirectionModel,
filter?: string,
skip?: number,
take?: number,
}): CancelablePromise<PagedMediaCollectionResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/collection/media',
            query: {
                'id': id,
                'dataTypeId': dataTypeId,
                'orderBy': orderBy,
                'orderDirection': orderDirection,
                'filter': filter,
                'skip': skip,
                'take': take,
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
    public static getItemMedia({
id,
}: {
id?: Array<string>,
}): CancelablePromise<Array<MediaItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/item/media',
            query: {
                'id': id,
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
            responseHeader: 'Umb-Generated-Resource',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
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
}): CancelablePromise<MediaResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/media/{id}',
            path: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
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
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
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
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
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
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putMediaByIdMoveToRecycleBin({
id,
}: {
id: string,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/media/{id}/move-to-recycle-bin',
            path: {
                'id': id,
            },
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putMediaByIdValidate({
id,
requestBody,
}: {
id: string,
requestBody?: UpdateMediaRequestModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/media/{id}/validate',
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
     * @returns any Success
     * @throws ApiError
     */
    public static getMediaConfiguration(): CancelablePromise<MediaConfigurationResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/media/configuration',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putMediaSort({
requestBody,
}: {
requestBody?: SortingRequestModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/media/sort',
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
     * @returns any Success
     * @throws ApiError
     */
    public static postMediaValidate({
requestBody,
}: {
requestBody?: CreateMediaRequestModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/media/validate',
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
     * @returns any Success
     * @throws ApiError
     */
    public static deleteRecycleBinMedia(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/recycle-bin/media',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteRecycleBinMediaById({
id,
}: {
id: string,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/recycle-bin/media/{id}',
            path: {
                'id': id,
            },
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns PagedMediaRecycleBinItemResponseModel Success
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
}): CancelablePromise<PagedMediaRecycleBinItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/recycle-bin/media/children',
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
     * @returns PagedMediaRecycleBinItemResponseModel Success
     * @throws ApiError
     */
    public static getRecycleBinMediaRoot({
skip,
take = 100,
}: {
skip?: number,
take?: number,
}): CancelablePromise<PagedMediaRecycleBinItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/recycle-bin/media/root',
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
            errors: {
                401: `The resource is protected and requires an authentication token`,
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
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

}
