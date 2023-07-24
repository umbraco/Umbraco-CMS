/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class PropertyTypeResource {

    /**
     * @returns boolean Success
     * @throws ApiError
     */
    public static getPropertyTypeIsUsed({
        contentTypeId,
        propertyAlias,
    }: {
        contentTypeId?: string,
        propertyAlias?: string,
    }): CancelablePromise<boolean> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/property-type/is-used',
            query: {
                'contentTypeId': contentTypeId,
                'propertyAlias': propertyAlias,
            },
            errors: {
                400: `Bad Request`,
            },
        });
    }

}
