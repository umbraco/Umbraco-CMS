/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ServerConfigurationResponseModel } from '../models/ServerConfigurationResponseModel';
import type { ServerInformationResponseModel } from '../models/ServerInformationResponseModel';
import type { ServerStatusResponseModel } from '../models/ServerStatusResponseModel';
import type { ServerTroubleshootingResponseModel } from '../models/ServerTroubleshootingResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ServerResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getServerConfiguration(): CancelablePromise<ServerConfigurationResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/server/configuration',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getServerInformation(): CancelablePromise<ServerInformationResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/server/information',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

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
    public static getServerTroubleshooting(): CancelablePromise<ServerTroubleshootingResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/server/troubleshooting',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

}
