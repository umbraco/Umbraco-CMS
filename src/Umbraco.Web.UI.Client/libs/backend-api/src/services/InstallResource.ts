/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DatabaseInstallResponseModel } from '../models/DatabaseInstallResponseModel';
import type { InstallSettingsResponseModel } from '../models/InstallSettingsResponseModel';
import type { InstallVResponseModel } from '../models/InstallVResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class InstallResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getInstallSettings(): CancelablePromise<InstallSettingsResponseModel> {
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
    public static postInstallSetup({
requestBody,
}: {
requestBody?: InstallVResponseModel,
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
    public static postInstallValidateDatabase({
requestBody,
}: {
requestBody?: DatabaseInstallResponseModel,
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
