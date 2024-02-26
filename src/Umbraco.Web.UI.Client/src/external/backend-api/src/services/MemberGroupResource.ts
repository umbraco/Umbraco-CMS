/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { MemberGroupItemResponseModel } from '../models/MemberGroupItemResponseModel';
import type { PagedNamedEntityTreeItemResponseModel } from '../models/PagedNamedEntityTreeItemResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MemberGroupResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getItemMemberGroup({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<MemberGroupItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/item/member-group',
            query: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns PagedNamedEntityTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeMemberGroupRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedNamedEntityTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/member-group/root',
            query: {
                'skip': skip,
                'take': take,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

}
