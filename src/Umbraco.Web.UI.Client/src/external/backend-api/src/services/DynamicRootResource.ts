/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DynamicRootRequestModel } from '../models/DynamicRootRequestModel';
import type { DynamicRootResponseModel } from '../models/DynamicRootResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DynamicRootResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postDynamicRootQuery({
        requestBody,
    }: {
        requestBody?: DynamicRootRequestModel,
    }): CancelablePromise<DynamicRootResponseModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/dynamic-root/query',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static getDynamicRootSteps(): CancelablePromise<Array<string>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/dynamic-root/steps',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

}
