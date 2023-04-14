/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedFileSystemTreeItemPresentationModel } from '../models/PagedFileSystemTreeItemPresentationModel';
import type { PartialViewItemResponseModel } from '../models/PartialViewItemResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class PartialViewResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getPartialViewItem({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<PartialViewItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/partial-view/item',
            query: {
                'id': id,
            },
        });
    }

    /**
     * @returns PagedFileSystemTreeItemPresentationModel Success
     * @throws ApiError
     */
    public static getTreePartialViewChildren({
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
            url: '/umbraco/management/api/v1/tree/partial-view/children',
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
    public static getTreePartialViewRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedFileSystemTreeItemPresentationModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/partial-view/root',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
