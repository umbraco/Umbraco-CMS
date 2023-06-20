/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedFileSystemTreeItemPresentationModel } from '../models/PagedFileSystemTreeItemPresentationModel';
import type { StaticFileItemResponseModel } from '../models/StaticFileItemResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class StaticFileResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getStaticFileItem({
path,
}: {
path?: Array<string>,
}): CancelablePromise<Array<StaticFileItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/static-file/item',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns PagedFileSystemTreeItemPresentationModel Success
     * @throws ApiError
     */
    public static getTreeStaticFileChildren({
path,
skip,
take = 100,
}: {
path?: string,
skip?: number,
take?: number,
}): CancelablePromise<PagedFileSystemTreeItemPresentationModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/static-file/children',
            query: {
                'path': path,
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns PagedFileSystemTreeItemPresentationModel Success
     * @throws ApiError
     */
    public static getTreeStaticFileRoot({
skip,
take = 100,
}: {
skip?: number,
take?: number,
}): CancelablePromise<PagedFileSystemTreeItemPresentationModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/static-file/root',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
