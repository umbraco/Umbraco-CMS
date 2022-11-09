/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DatabaseInstall } from '../models/DatabaseInstall';
import type { Install } from '../models/Install';
import type { InstallSettings } from '../models/InstallSettings';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class InstallResource {

    /**
     * @returns InstallSettings Success
     * @throws ApiError
     */
    public static settings(): CancelablePromise<InstallSettings> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/install/settings',
            errors: {
                400: `Bad Request`,
                428: `Client Error`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static setup({
        requestBody,
    }: {
        requestBody?: Install,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/install/setup',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                428: `Client Error`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static validateDatabase({
        requestBody,
    }: {
        requestBody?: DatabaseInstall,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/install/validate-database',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

}
