/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedTelemetryResponseModel } from '../models/PagedTelemetryResponseModel';
import type { TelemetryRequestModel } from '../models/TelemetryRequestModel';
import type { TelemetryResponseModel } from '../models/TelemetryResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class TelemetryResource {

    /**
     * @returns PagedTelemetryResponseModel Success
     * @throws ApiError
     */
    public static getTelemetry({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedTelemetryResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/telemetry',
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
    public static getTelemetryLevel(): CancelablePromise<TelemetryResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/telemetry/level',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postTelemetryLevel({
        requestBody,
    }: {
        requestBody?: TelemetryRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/telemetry/level',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

}
