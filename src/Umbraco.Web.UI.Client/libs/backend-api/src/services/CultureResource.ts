/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedCulture } from '../models/PagedCulture';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class CultureResource {

    /**
     * @returns PagedCulture Success
     * @throws ApiError
     */
    public static getCulture({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedCulture> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/culture',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
