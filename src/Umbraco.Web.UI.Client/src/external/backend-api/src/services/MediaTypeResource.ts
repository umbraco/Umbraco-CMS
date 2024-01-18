/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AvailableMediaTypeCompositionResponseModel } from '../models/AvailableMediaTypeCompositionResponseModel';
import type { CopyMediaTypeRequestModel } from '../models/CopyMediaTypeRequestModel';
import type { CreateFolderRequestModel } from '../models/CreateFolderRequestModel';
import type { CreateMediaTypeRequestModel } from '../models/CreateMediaTypeRequestModel';
import type { FolderResponseModel } from '../models/FolderResponseModel';
import type { MediaTypeCompositionRequestModel } from '../models/MediaTypeCompositionRequestModel';
import type { MediaTypeCompositionResponseModel } from '../models/MediaTypeCompositionResponseModel';
import type { MediaTypeItemResponseModel } from '../models/MediaTypeItemResponseModel';
import type { MediaTypeResponseModel } from '../models/MediaTypeResponseModel';
import type { MoveMediaTypeRequestModel } from '../models/MoveMediaTypeRequestModel';
import type { PagedMediaTypeTreeItemResponseModel } from '../models/PagedMediaTypeTreeItemResponseModel';
import type { UpdateFolderResponseModel } from '../models/UpdateFolderResponseModel';
import type { UpdateMediaTypeRequestModel } from '../models/UpdateMediaTypeRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MediaTypeResource {

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postMediaType({
requestBody,
}: {
requestBody?: CreateMediaTypeRequestModel,
}): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/media-type',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Location',
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
    public static getMediaTypeById({
id,
}: {
id: string,
}): CancelablePromise<MediaTypeResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/media-type/{id}',
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
    public static deleteMediaTypeById({
id,
}: {
id: string,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/media-type/{id}',
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
    public static putMediaTypeById({
id,
requestBody,
}: {
id: string,
requestBody?: UpdateMediaTypeRequestModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/media-type/{id}',
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
     * @returns any Success
     * @throws ApiError
     */
    public static getMediaTypeByIdCompositionReferences({
id,
}: {
id: string,
}): CancelablePromise<Array<MediaTypeCompositionResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/media-type/{id}/composition-references',
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
     * @returns string Created
     * @throws ApiError
     */
    public static postMediaTypeByIdCopy({
id,
requestBody,
}: {
id: string,
requestBody?: CopyMediaTypeRequestModel,
}): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/media-type/{id}/copy',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Location',
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
    public static putMediaTypeByIdMove({
id,
requestBody,
}: {
id: string,
requestBody?: MoveMediaTypeRequestModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/media-type/{id}/move',
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
     * @returns any Success
     * @throws ApiError
     */
    public static postMediaTypeAvailableCompositions({
requestBody,
}: {
requestBody?: MediaTypeCompositionRequestModel,
}): CancelablePromise<Array<AvailableMediaTypeCompositionResponseModel>> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/media-type/available-compositions',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postMediaTypeFolder({
requestBody,
}: {
requestBody?: CreateFolderRequestModel,
}): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/media-type/folder',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Location',
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
    public static getMediaTypeFolderById({
id,
}: {
id: string,
}): CancelablePromise<FolderResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/media-type/folder/{id}',
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
    public static deleteMediaTypeFolderById({
id,
}: {
id: string,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/media-type/folder/{id}',
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
    public static putMediaTypeFolderById({
id,
requestBody,
}: {
id: string,
requestBody?: UpdateFolderResponseModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/media-type/folder/{id}',
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
     * @returns any Success
     * @throws ApiError
     */
    public static getMediaTypeItem({
id,
}: {
id?: Array<string>,
}): CancelablePromise<Array<MediaTypeItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/media-type/item',
            query: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns PagedMediaTypeTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeMediaTypeChildren({
parentId,
skip,
take = 100,
foldersOnly = false,
}: {
parentId?: string,
skip?: number,
take?: number,
foldersOnly?: boolean,
}): CancelablePromise<PagedMediaTypeTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media-type/children',
            query: {
                'parentId': parentId,
                'skip': skip,
                'take': take,
                'foldersOnly': foldersOnly,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns PagedMediaTypeTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeMediaTypeRoot({
skip,
take = 100,
foldersOnly = false,
}: {
skip?: number,
take?: number,
foldersOnly?: boolean,
}): CancelablePromise<PagedMediaTypeTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media-type/root',
            query: {
                'skip': skip,
                'take': take,
                'foldersOnly': foldersOnly,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

}
