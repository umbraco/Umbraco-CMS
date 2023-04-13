/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { StaticFileItemResponseModel } from '../models/StaticFileItemResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class V1Resource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getItem({
        path,
    }: {
        path?: Array<string>,
    }): CancelablePromise<Array<StaticFileItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/item',
            query: {
                'path': path,
            },
        });
    }

}
