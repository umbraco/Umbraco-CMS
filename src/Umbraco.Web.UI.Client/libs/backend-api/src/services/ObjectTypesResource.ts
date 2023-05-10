/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedObjectTypeResponseModel } from '../models/PagedObjectTypeResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ObjectTypesResource {

    /**
     * @returns PagedObjectTypeResponseModel Success
     * @throws ApiError
     */
    public static getObjectTypes({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedObjectTypeResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/object-types',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
