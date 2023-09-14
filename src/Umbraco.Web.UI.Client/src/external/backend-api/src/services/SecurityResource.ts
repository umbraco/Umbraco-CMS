/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { LoginRequestModel } from '../models/LoginRequestModel';
import type { ResetPasswordRequestModel } from '../models/ResetPasswordRequestModel';
import type { ResetPasswordTokenRequestModel } from '../models/ResetPasswordTokenRequestModel';
import type { VerifyResetPasswordTokenRequestModel } from '../models/VerifyResetPasswordTokenRequestModel';

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

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postSecurityForgotPassword({
        requestBody,
    }: {
        requestBody?: ResetPasswordRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/security/forgot-password',
            body: requestBody,
            mediaType: 'application/json',
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
                404: `Not Found`,
            },
        });
    }

}
