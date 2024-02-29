/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class PreviewResource {
    /**
     * @returns string Success
     * @throws ApiError
     */
    public static deletePreview(): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/preview',
            responseHeader: 'Umb-Notifications',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }
    /**
     * @returns string Success
     * @throws ApiError
     */
    public static postPreview(): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/preview',
            responseHeader: 'Umb-Notifications',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }
}
