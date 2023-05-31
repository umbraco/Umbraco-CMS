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
     * @returns any Success
     * @throws ApiError
     */
    public static postModelsBuilderBuild(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/models-builder/build',
            errors: {
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
        });
    }

}
