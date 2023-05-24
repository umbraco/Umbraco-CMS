/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ServerStatusResponseModel } from '../models/ServerStatusResponseModel';
import type { VersionResponseModel } from '../models/VersionResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ServerResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getServerStatus(): CancelablePromise<ServerStatusResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/server/status',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getServerVersion(): CancelablePromise<VersionResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/server/version',
            errors: {
                400: `Bad Request`,
            },
        });
    }

}
