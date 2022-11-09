/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ProfilingStatus } from '../models/ProfilingStatus';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ProfilingResource {

    /**
     * @returns ProfilingStatus Success
     * @throws ApiError
     */
    public static status(): CancelablePromise<ProfilingStatus> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/profiling/status',
        });
    }

}
