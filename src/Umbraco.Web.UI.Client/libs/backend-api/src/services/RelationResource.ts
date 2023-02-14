/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedRelationModel } from '../models/PagedRelationModel';
import type { RelationModel } from '../models/RelationModel';

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
    }): CancelablePromise<RelationModel> {
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
     * @returns PagedRelationModel Success
     * @throws ApiError
     */
    public static getRelationChildRelationByChildId({
        childId,
        skip,
        take,
        relationTypeAlias = '',
    }: {
        childId: number,
        skip?: number,
        take?: number,
        relationTypeAlias?: string,
    }): CancelablePromise<PagedRelationModel> {
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

}
