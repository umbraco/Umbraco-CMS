/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { Language } from '../models/Language';
import type { PagedLanguage } from '../models/PagedLanguage';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class LanguageResource {

    /**
     * @returns any Created
     * @throws ApiError
     */
    public static postLanguage({
        requestBody,
    }: {
        requestBody?: Language,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/language',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns PagedLanguage Success
     * @throws ApiError
     */
    public static getLanguage({
        skip,
        take,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedLanguage> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/language',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns Language Success
     * @throws ApiError
     */
    public static getLanguageById({
        id,
    }: {
        id: number,
    }): CancelablePromise<Language> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/language/{id}',
            path: {
                'id': id,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteLanguageById({
        id,
    }: {
        id: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/language/{id}',
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
    public static putLanguageById({
        id,
        requestBody,
    }: {
        id: number,
        requestBody?: Language,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/language/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                404: `Not Found`,
            },
        });
    }

}
