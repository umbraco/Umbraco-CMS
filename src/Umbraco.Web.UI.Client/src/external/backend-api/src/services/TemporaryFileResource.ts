/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { TemporaryFileConfigurationResponseModel } from '../models/TemporaryFileConfigurationResponseModel';
import type { TemporaryFileResponseModel } from '../models/TemporaryFileResponseModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class TemporaryFileResource {
    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postTemporaryFile({
        formData,
    }: {
        formData?: {
            Id?: string;
            File?: Blob;
        },
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/temporary-file',
            formData: formData,
            mediaType: 'multipart/form-data',
            responseHeader: 'Umb-Generated-Resource',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTemporaryFileById({
        id,
    }: {
        id: string,
    }): CancelablePromise<TemporaryFileResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/temporary-file/{id}',
            path: {
                'id': id,
            },
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns string Success
     * @throws ApiError
     */
    public static deleteTemporaryFileById({
        id,
    }: {
        id: string,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/temporary-file/{id}',
            path: {
                'id': id,
            },
            responseHeader: 'Umb-Notifications',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTemporaryFileConfiguration(): CancelablePromise<TemporaryFileConfigurationResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/temporary-file/configuration',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }
}
