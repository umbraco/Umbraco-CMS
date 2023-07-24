/* generated using openapi-typescript-codegen -- do no edit */
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
    public static getTrackedReferenceById({
        id,
        skip,
        take = 20,
        filterMustBeIsDependency = false,
    }: {
        id: string,
        skip?: number,
        take?: number,
        filterMustBeIsDependency?: boolean,
    }): CancelablePromise<PagedRelationItemResponseModel> {
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
     * @returns PagedRelationItemResponseModel Success
     * @throws ApiError
     */
    public static getTrackedReferenceDescendantsByParentId({
        parentId,
        skip,
        take = 20,
        filterMustBeIsDependency = true,
    }: {
        parentId: string,
        skip?: number,
        take?: number,
        filterMustBeIsDependency?: boolean,
    }): CancelablePromise<PagedRelationItemResponseModel> {
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
     * @returns PagedRelationItemResponseModel Success
     * @throws ApiError
     */
    public static getTrackedReferenceItem({
        id,
        skip,
        take = 20,
        filterMustBeIsDependency = true,
    }: {
        id?: Array<string>,
        skip?: number,
        take?: number,
        filterMustBeIsDependency?: boolean,
    }): CancelablePromise<PagedRelationItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tracked-reference/item',
            query: {
                'id': id,
                'skip': skip,
                'take': take,
                'filterMustBeIsDependency': filterMustBeIsDependency,
            },
        });
    }

}
