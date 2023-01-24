/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedRelationItem } from '../models/PagedRelationItem';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class TrackedReferenceResource {

    /**
     * @returns PagedRelationItem Success
     * @throws ApiError
     */
    public static getTrackedReferenceById({
        id,
        skip,
        take,
        filterMustBeIsDependency,
    }: {
        id: number,
        skip?: number,
        take?: number,
        filterMustBeIsDependency?: boolean,
    }): CancelablePromise<PagedRelationItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tracked-reference/{id}',
            path: {
                'id': id,
            },
            query: {
                'skip': skip,
                'take': take,
                'filterMustBeIsDependency': filterMustBeIsDependency,
            },
        });
    }

    /**
     * @returns PagedRelationItem Success
     * @throws ApiError
     */
    public static getTrackedReferenceDescendantsByParentId({
        parentId,
        skip,
        take,
        filterMustBeIsDependency,
    }: {
        parentId: number,
        skip?: number,
        take?: number,
        filterMustBeIsDependency?: boolean,
    }): CancelablePromise<PagedRelationItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tracked-reference/descendants/{parentId}',
            path: {
                'parentId': parentId,
            },
            query: {
                'skip': skip,
                'take': take,
                'filterMustBeIsDependency': filterMustBeIsDependency,
            },
        });
    }

    /**
     * @returns PagedRelationItem Success
     * @throws ApiError
     */
    public static getTrackedReferenceItem({
        ids,
        skip,
        take,
        filterMustBeIsDependency,
    }: {
        ids?: Array<number>,
        skip?: number,
        take?: number,
        filterMustBeIsDependency?: boolean,
    }): CancelablePromise<PagedRelationItem> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tracked-reference/item',
            query: {
                'ids': ids,
                'skip': skip,
                'take': take,
                'filterMustBeIsDependency': filterMustBeIsDependency,
            },
        });
    }

}
