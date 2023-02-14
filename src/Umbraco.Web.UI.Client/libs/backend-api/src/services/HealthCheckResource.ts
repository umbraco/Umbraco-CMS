/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { HealthCheckActionModel } from '../models/HealthCheckActionModel';
import type { HealthCheckGroupModel } from '../models/HealthCheckGroupModel';
import type { HealthCheckGroupWithResultModel } from '../models/HealthCheckGroupWithResultModel';
import type { HealthCheckResultModel } from '../models/HealthCheckResultModel';
import type { PagedHealthCheckGroupModelBaseModel } from '../models/PagedHealthCheckGroupModelBaseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class HealthCheckResource {

    /**
     * @returns PagedHealthCheckGroupModelBaseModel Success
     * @throws ApiError
     */
    public static getHealthCheckGroup({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedHealthCheckGroupModelBaseModel> {
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
     * @returns any Success
     * @throws ApiError
     */
    public static getHealthCheckGroupByName({
        name,
    }: {
        name: string,
    }): CancelablePromise<HealthCheckGroupModel> {
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
     * @returns any Success
     * @throws ApiError
     */
    public static postHealthCheckGroupByNameCheck({
        name,
    }: {
        name: string,
    }): CancelablePromise<HealthCheckGroupWithResultModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/health-check-group/{name}/check',
            path: {
                'name': name,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postHealthCheckExecuteAction({
        requestBody,
    }: {
        requestBody?: HealthCheckActionModel,
    }): CancelablePromise<HealthCheckResultModel> {
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
