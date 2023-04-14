/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedBooleanModel } from '../models/PagedBooleanModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class PropertyTypeResource {

    /**
     * @returns PagedBooleanModel Success
     * @throws ApiError
     */
    public static getPropertyTypeIsUsed({
contentTypeId,
propertyAlias,
}: {
contentTypeId?: string,
propertyAlias?: string,
}): CancelablePromise<PagedBooleanModel> {
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
