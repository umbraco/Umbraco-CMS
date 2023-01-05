/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { HealthCheckAction } from '../models/HealthCheckAction';
import type { HealthCheckGroupWithResult } from '../models/HealthCheckGroupWithResult';
import type { HealthCheckResult } from '../models/HealthCheckResult';
import type { PagedHealthCheckGroup } from '../models/PagedHealthCheckGroup';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class HealthCheckResource {

    /**
     * @returns PagedHealthCheckGroup Success
     * @throws ApiError
     */
    public static getHealthCheckGroup({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedHealthCheckGroup> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/health-check-group',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns HealthCheckGroupWithResult Success
     * @throws ApiError
     */
    public static getHealthCheckGroupByName({
        name,
    }: {
        name: string,
    }): CancelablePromise<HealthCheckGroupWithResult> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/health-check-group/{name}',
            path: {
                'name': name,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns HealthCheckResult Success
     * @throws ApiError
     */
    public static postHealthCheckExecuteAction({
        requestBody,
    }: {
        requestBody?: HealthCheckAction,
    }): CancelablePromise<HealthCheckResult> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/health-check/execute-action',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

}
