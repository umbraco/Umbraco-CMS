/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DocumentTypeTreeItemModel } from '../models/DocumentTypeTreeItemModel';
import type { FolderTreeItemModel } from '../models/FolderTreeItemModel';
import type { PagedEntityTreeItemModel } from '../models/PagedEntityTreeItemModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class RelationTypeResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTreeRelationTypeItem({
        key,
    }: {
        key?: Array<string>,
    }): CancelablePromise<Array<(FolderTreeItemModel | DocumentTypeTreeItemModel)>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/relation-type/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItemModel Success
     * @throws ApiError
     */
    public static getTreeRelationTypeRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedEntityTreeItemModel> {
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
