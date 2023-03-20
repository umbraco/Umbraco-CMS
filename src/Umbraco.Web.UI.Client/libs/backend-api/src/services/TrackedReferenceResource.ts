/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedRelationItemResponseModel } from '../models/PagedRelationItemResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class TrackedReferenceResource {

    /**
     * @returns PagedRelationItemResponseModel Success
     * @throws ApiError
     */
    public static getTrackedReferenceByKey({
key,
skip,
take = 20,
filterMustBeIsDependency = false,
}: {
key: string,
skip?: number,
take?: number,
filterMustBeIsDependency?: boolean,
}): CancelablePromise<PagedRelationItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tracked-reference/{key}',
            path: {
                'key': key,
            },
            query: {
                'skip': skip,
                'take': take,
                'filterMustBeIsDependency': filterMustBeIsDependency,
            },
        });
    }

    /**
     * @returns PagedRelationItemResponseModel Success
     * @throws ApiError
     */
    public static getTrackedReferenceDescendantsByParentKey({
parentKey,
skip,
take,
filterMustBeIsDependency = true,
}: {
parentKey: string,
skip?: number,
take?: number,
filterMustBeIsDependency?: boolean,
}): CancelablePromise<PagedRelationItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tracked-reference/descendants/{parentKey}',
            path: {
                'parentKey': parentKey,
            },
            query: {
                'skip': skip,
                'take': take,
                'filterMustBeIsDependency': filterMustBeIsDependency,
            },
        });
    }

    /**
     * @returns PagedRelationItemResponseModel Success
     * @throws ApiError
     */
    public static getTrackedReferenceItem({
key,
skip,
take = 20,
filterMustBeIsDependency = true,
}: {
key?: Array<string>,
skip?: number,
take?: number,
filterMustBeIsDependency?: boolean,
}): CancelablePromise<PagedRelationItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tracked-reference/item',
            query: {
                'key': key,
                'skip': skip,
                'take': take,
                'filterMustBeIsDependency': filterMustBeIsDependency,
            },
        });
    }

}
