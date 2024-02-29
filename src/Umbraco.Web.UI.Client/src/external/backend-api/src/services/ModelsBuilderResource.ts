/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ModelsBuilderResponseModel } from '../models/ModelsBuilderResponseModel';
import type { OutOfDateStatusResponseModel } from '../models/OutOfDateStatusResponseModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class ModelsBuilderResource {
    /**
     * @returns string Success
     * @throws ApiError
     */
    public static postModelsBuilderBuild(): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/models-builder/build',
            responseHeader: 'Umb-Notifications',
            errors: {
                401: `The resource is protected and requires an authentication token`,
                428: `Client Error`,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getModelsBuilderDashboard(): CancelablePromise<ModelsBuilderResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/models-builder/dashboard',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getModelsBuilderStatus(): CancelablePromise<OutOfDateStatusResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/models-builder/status',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }
}
