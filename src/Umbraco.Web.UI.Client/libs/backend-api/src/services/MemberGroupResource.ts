/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { MemberGroupItemReponseModel } from '../models/MemberGroupItemReponseModel';
import type { PagedEntityTreeItemResponseModel } from '../models/PagedEntityTreeItemResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MemberGroupResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getMemberGroupItem({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<MemberGroupItemReponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/member-group/item',
            query: {
                'id': id,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeMemberGroupRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedEntityTreeItemResponseModel> {
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
