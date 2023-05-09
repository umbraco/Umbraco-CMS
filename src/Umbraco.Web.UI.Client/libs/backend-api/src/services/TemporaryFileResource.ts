/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { TemporaryFileResponseModel } from '../models/TemporaryFileResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class TemporaryFileResource {

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postTemporaryfile({
        formData,
    }: {
        formData?: {
            Id?: string;
            File?: Blob;
        },
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/temporaryfile',
            formData: formData,
            mediaType: 'multipart/form-data',
            responseHeader: 'Location',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTemporaryfileById({
        id,
    }: {
        id: string,
    }): CancelablePromise<TemporaryFileResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/temporaryfile/{id}',
            path: {
                'id': id,
            },
            errors: {
                400: `Bad Request`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteTemporaryfileById({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/temporaryfile/{id}',
            path: {
                'id': id,
            },
            errors: {
                400: `Bad Request`,
                404: `Not Found`,
            },
        });
    }

}
