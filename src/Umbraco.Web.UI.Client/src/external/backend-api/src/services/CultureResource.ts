/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedCultureReponseModel } from '../models/PagedCultureReponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class CultureResource {

    /**
     * @returns PagedCultureReponseModel Success
     * @throws ApiError
     */
    public static getCulture({
skip,
take = 100,
}: {
skip?: number,
take?: number,
}): CancelablePromise<PagedCultureReponseModel> {
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
