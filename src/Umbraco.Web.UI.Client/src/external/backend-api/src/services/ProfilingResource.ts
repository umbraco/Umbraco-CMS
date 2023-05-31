/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ProfilingStatusRequestModel } from '../models/ProfilingStatusRequestModel';
import type { ProfilingStatusResponseModel } from '../models/ProfilingStatusResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ProfilingResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getProfilingStatus(): CancelablePromise<ProfilingStatusResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/profiling/status',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putProfilingStatus({
        requestBody,
    }: {
        requestBody?: ProfilingStatusRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/profiling/status',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

}
