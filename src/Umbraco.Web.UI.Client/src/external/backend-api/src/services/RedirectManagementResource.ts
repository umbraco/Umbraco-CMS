/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedRedirectUrlResponseModel } from '../models/PagedRedirectUrlResponseModel';
import type { RedirectStatusModel } from '../models/RedirectStatusModel';
import type { RedirectUrlStatusResponseModel } from '../models/RedirectUrlStatusResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class RedirectManagementResource {

    /**
     * @returns PagedRedirectUrlResponseModel Success
     * @throws ApiError
     */
    public static getRedirectManagement({
        filter,
        skip,
        take = 100,
    }: {
        filter?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedRedirectUrlResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/redirect-management',
            query: {
                'filter': filter,
                'skip': skip,
                'take': take,
            },
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns PagedRedirectUrlResponseModel Success
     * @throws ApiError
     */
    public static getRedirectManagementById({
        id,
        skip,
        take = 100,
    }: {
        id: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedRedirectUrlResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/redirect-management/{id}',
            path: {
                'id': id,
            },
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteRedirectManagementById({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/redirect-management/{id}',
            path: {
                'id': id,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getRedirectManagementStatus(): CancelablePromise<RedirectUrlStatusResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/redirect-management/status',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postRedirectManagementStatus({
        status,
    }: {
        status?: RedirectStatusModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/redirect-management/status',
            query: {
                'status': status,
            },
        });
    }

}
