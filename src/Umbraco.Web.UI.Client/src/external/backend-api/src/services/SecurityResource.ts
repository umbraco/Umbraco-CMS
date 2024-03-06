/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ResetPasswordRequestModel } from '../models/ResetPasswordRequestModel';
import type { ResetPasswordTokenRequestModel } from '../models/ResetPasswordTokenRequestModel';
import type { SecurityConfigurationResponseModel } from '../models/SecurityConfigurationResponseModel';
import type { VerifyResetPasswordTokenRequestModel } from '../models/VerifyResetPasswordTokenRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class SecurityResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getSecurityConfiguration(): CancelablePromise<SecurityConfigurationResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/security/configuration',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static postSecurityForgotPassword({
        requestBody,
    }: {
        requestBody?: ResetPasswordRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/security/forgot-password',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns void
     * @throws ApiError
     */
    public static postSecurityForgotPasswordReset({
        requestBody,
    }: {
        requestBody?: ResetPasswordTokenRequestModel,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/security/forgot-password/reset',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns void
     * @throws ApiError
     */
    public static postSecurityForgotPasswordVerify({
        requestBody,
    }: {
        requestBody?: (VerifyResetPasswordTokenRequestModel | ResetPasswordTokenRequestModel),
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/security/forgot-password/verify',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

}
