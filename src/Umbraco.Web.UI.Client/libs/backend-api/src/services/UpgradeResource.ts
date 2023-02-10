/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { UpgradeSettingsModel } from '../models/UpgradeSettingsModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class UpgradeResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postUpgradeAuthorize(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/upgrade/authorize',
            errors: {
                428: `Client Error`,
                500: `Server Error`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getUpgradeSettings(): CancelablePromise<UpgradeSettingsModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/upgrade/settings',
            errors: {
                428: `Client Error`,
            },
        });
    }

}
