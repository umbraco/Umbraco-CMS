/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class PublishedCacheResource {

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static postPublishedCacheCollect(): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/published-cache/collect',
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
    public static postPublishedCacheRebuild(): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/published-cache/rebuild',
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
    public static postPublishedCacheReload(): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/published-cache/reload',
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
    public static getPublishedCacheStatus(): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/published-cache/status',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

}
