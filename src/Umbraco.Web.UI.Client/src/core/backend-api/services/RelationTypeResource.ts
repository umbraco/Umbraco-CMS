/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { FolderTreeItem } from '../models/FolderTreeItem';
import type { PagedEntityTreeItem } from '../models/PagedEntityTreeItem';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class RelationTypeResource {

    /**
     * @returns FolderTreeItem Success
     * @throws ApiError
     */
    public static getTreeRelationTypeItem({
        key,
    }: {
        key?: Array<string>,
    }): CancelablePromise<Array<FolderTreeItem>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/relation-type/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItem Success
     * @throws ApiError
     */
    public static getTreeRelationTypeRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedEntityTreeItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/relation-type/root',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
