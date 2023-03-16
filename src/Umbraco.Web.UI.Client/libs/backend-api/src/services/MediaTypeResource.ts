/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DocumentTypeTreeItemResponseModel } from '../models/DocumentTypeTreeItemResponseModel';
import type { FolderTreeItemResponseModel } from '../models/FolderTreeItemResponseModel';
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
    public static getMediaTypeByKey({
key,
}: {
key: string,
}): CancelablePromise<MediaTypeResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/media-type/{key}',
            path: {
                'key': key,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns PagedFolderTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeMediaTypeChildren({
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
            url: '/umbraco/management/api/v1/tree/media-type/children',
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
    public static getTreeMediaTypeItem({
key,
}: {
key?: Array<string>,
}): CancelablePromise<Array<(FolderTreeItemResponseModel | DocumentTypeTreeItemResponseModel)>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/media-type/item',
            query: {
                'key': key,
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
