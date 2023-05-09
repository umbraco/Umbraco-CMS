/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { MemberTypeItemResponseModel } from '../models/MemberTypeItemResponseModel';
import type { PagedEntityTreeItemResponseModel } from '../models/PagedEntityTreeItemResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MemberTypeResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getMemberTypeItem({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<MemberTypeItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/member-type/item',
            query: {
                'id': id,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeMemberTypeRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedEntityTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/member-type/root',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
