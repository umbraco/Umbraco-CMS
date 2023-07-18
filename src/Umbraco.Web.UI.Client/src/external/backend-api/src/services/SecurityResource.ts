/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { LoginRequestModel } from '../models/LoginRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class SecurityResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getSecurityBackOfficeAuthorize(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/security/back-office/authorize',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postSecurityBackOfficeLogin({
        requestBody,
    }: {
        requestBody?: LoginRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/security/back-office/login',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

}
