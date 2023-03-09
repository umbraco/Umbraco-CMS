/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedTelemetryModel } from '../models/PagedTelemetryModel';
import type { TelemetryModel } from '../models/TelemetryModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class TelemetryResource {

    /**
     * @returns PagedTelemetryModel Success
     * @throws ApiError
     */
    public static getTelemetry({
        skip,
        take,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedTelemetryModel> {
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
    public static getTelemetryLevel(): CancelablePromise<TelemetryModel> {
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
        requestBody?: TelemetryModel,
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
