/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { UpgradeSettingsResponseModel } from '../models/UpgradeSettingsResponseModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class UpgradeResource {
    /**
     * @returns string Success
     * @throws ApiError
     */
    public static postUpgradeAuthorize(): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/upgrade/authorize',
            responseHeader: 'Umb-Notifications',
            errors: {
                401: `The resource is protected and requires an authentication token`,
                428: `Client Error`,
                500: `Server Error`,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getUpgradeSettings(): CancelablePromise<UpgradeSettingsResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/upgrade/settings',
            errors: {
                401: `The resource is protected and requires an authentication token`,
                428: `Client Error`,
            },
        });
    }
}
