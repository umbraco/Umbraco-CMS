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
                401: `The resource is protected and requires an authentication token`,
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
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }
    /**
     * @returns string Success
     * @throws ApiError
     */
    public static deleteRedirectManagementById({
        id,
    }: {
        id: string,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/redirect-management/{id}',
            path: {
                'id': id,
            },
            responseHeader: 'Umb-Notifications',
            errors: {
                401: `The resource is protected and requires an authentication token`,
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
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }
    /**
     * @returns string Success
     * @throws ApiError
     */
    public static postRedirectManagementStatus({
        status,
    }: {
        status?: RedirectStatusModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/redirect-management/status',
            query: {
                'status': status,
            },
            responseHeader: 'Umb-Notifications',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }
}
