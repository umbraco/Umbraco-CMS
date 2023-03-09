/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ModelsBuilderModel } from '../models/ModelsBuilderModel';
import type { OutOfDateStatusModel } from '../models/OutOfDateStatusModel';

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
    public static getModelsBuilderDashboard(): CancelablePromise<ModelsBuilderModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/models-builder/dashboard',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getModelsBuilderStatus(): CancelablePromise<OutOfDateStatusModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/models-builder/status',
        });
    }

}
