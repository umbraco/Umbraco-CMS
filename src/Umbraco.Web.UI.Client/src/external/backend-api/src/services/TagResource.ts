/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedTagResponseModel } from '../models/PagedTagResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class TagResource {

    /**
     * @returns PagedTagResponseModel Success
     * @throws ApiError
     */
    public static getTag({
        query,
        tagGroup,
        culture,
        skip,
        take = 100,
    }: {
        query?: string,
        tagGroup?: string,
        culture?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedTagResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tag',
            query: {
                'query': query,
                'tagGroup': tagGroup,
                'culture': culture,
                'skip': skip,
                'take': take,
            },
        });
    }

}
