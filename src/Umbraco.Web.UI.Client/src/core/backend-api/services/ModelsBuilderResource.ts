/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreatedResult } from '../models/CreatedResult';
import type { ModelsBuilder } from '../models/ModelsBuilder';
import type { OutOfDateStatus } from '../models/OutOfDateStatus';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ModelsBuilderResource {

    /**
     * @returns CreatedResult Created
     * @throws ApiError
     */
    public static buildModels(): CancelablePromise<CreatedResult> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/models-builder/build',
            errors: {
                428: `Client Error`,
            },
        });
    }

    /**
     * @returns ModelsBuilder Success
     * @throws ApiError
     */
    public static getDashboard(): CancelablePromise<ModelsBuilder> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/models-builder/dashboard',
        });
    }

    /**
     * @returns OutOfDateStatus Success
     * @throws ApiError
     */
    public static getModelsOutOfDateStatus(): CancelablePromise<OutOfDateStatus> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/models-builder/status',
        });
    }

}
