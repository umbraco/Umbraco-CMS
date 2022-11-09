/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { EntityTreeItem } from '../models/EntityTreeItem';
import type { PagedEntityTreeItem } from '../models/PagedEntityTreeItem';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MemberGroupResource {

    /**
     * @returns EntityTreeItem Success
     * @throws ApiError
     */
    public static getUmbracoManagementApiV1TreeMemberGroupItem({
        key,
    }: {
        key?: Array<string>,
    }): CancelablePromise<Array<EntityTreeItem>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/member-group/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItem Success
     * @throws ApiError
     */
    public static getUmbracoManagementApiV1TreeMemberGroupRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedEntityTreeItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/member-group/root',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
