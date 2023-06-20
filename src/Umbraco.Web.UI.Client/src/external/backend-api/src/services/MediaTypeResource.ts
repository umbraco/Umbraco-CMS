/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { MediaTypeItemResponseModel } from '../models/MediaTypeItemResponseModel';
import type { MediaTypeResponseModel } from '../models/MediaTypeResponseModel';
import type { PagedFolderTreeItemResponseModel } from '../models/PagedFolderTreeItemResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MediaTypeResource {

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
        });
    }

    /**
     * @returns PagedFolderTreeItemResponseModel Success
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
}): CancelablePromise<PagedFolderTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media-type/children',
            query: {
                'parentId': parentId,
                'skip': skip,
                'take': take,
                'foldersOnly': foldersOnly,
            },
        });
    }

    /**
     * @returns PagedFolderTreeItemResponseModel Success
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
}): CancelablePromise<PagedFolderTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media-type/root',
            query: {
                'skip': skip,
                'take': take,
                'foldersOnly': foldersOnly,
            },
        });
    }

}
