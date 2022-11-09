/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class SecurityResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static authorize(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/security/back-office/authorize',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static authorize1(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/security/back-office/authorize',
        });
    }

}
