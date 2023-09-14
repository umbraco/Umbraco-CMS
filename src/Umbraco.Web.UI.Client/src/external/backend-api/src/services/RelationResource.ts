/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedRelationResponseModel } from '../models/PagedRelationResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class RelationResource {

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
        childId: string,
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
            errors: {
                404: `Not Found`,
            },
        });
    }

}
