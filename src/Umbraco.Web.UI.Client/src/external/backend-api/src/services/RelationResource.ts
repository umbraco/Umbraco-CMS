/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedRelationResponseModel } from '../models/PagedRelationResponseModel';
import type { RelationResponseModel } from '../models/RelationResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class RelationResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getRelationById({
        id,
    }: {
        id: number,
    }): CancelablePromise<RelationResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/relation/{id}',
            path: {
                'id': id,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns PagedRelationResponseModel Success
     * @throws ApiError
     */
    public static getRelationChildRelationByChildId({
        childId,
        skip,
        take = 100,
        relationTypeAlias = '',
    }: {
        childId: number,
        skip?: number,
        take?: number,
        relationTypeAlias?: string,
    }): CancelablePromise<PagedRelationResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/relation/child-relation/{childId}',
            path: {
                'childId': childId,
            },
            query: {
                'skip': skip,
                'take': take,
                'relationTypeAlias': relationTypeAlias,
            },
        });
    }

    /**
     * @returns PagedRelationResponseModel Success
     * @throws ApiError
     */
    public static getRelationTypeById({
        id,
        skip,
        take = 100,
    }: {
        id: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedRelationResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/relation/type/{id}',
            path: {
                'id': id,
            },
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
