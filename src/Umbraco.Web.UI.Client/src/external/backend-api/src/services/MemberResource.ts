/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { MemberItemResponseModel } from '../models/MemberItemResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MemberResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getMemberItem({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<MemberItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/member/item',
            query: {
                'id': id,
            },
        });
    }

}
