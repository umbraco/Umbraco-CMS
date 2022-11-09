/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ServerStatus } from '../models/ServerStatus';
import type { Version } from '../models/Version';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ServerResource {

    /**
     * @returns ServerStatus Success
     * @throws ApiError
     */
    public static getUmbracoManagementApiV1ServerStatus(): CancelablePromise<ServerStatus> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/server/status',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns Version Success
     * @throws ApiError
     */
    public static getUmbracoManagementApiV1ServerVersion(): CancelablePromise<Version> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/server/version',
            errors: {
                400: `Bad Request`,
            },
        });
    }

}
