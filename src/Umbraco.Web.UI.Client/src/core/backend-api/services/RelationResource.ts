/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedRelation } from '../models/PagedRelation';
import type { Relation } from '../models/Relation';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class RelationResource {

    /**
     * @returns Relation Success
     * @throws ApiError
     */
    public static byId({
        id,
    }: {
        id: number,
    }): CancelablePromise<Relation> {
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
     * @returns PagedRelation Success
     * @throws ApiError
     */
    public static byChild({
        childId,
        skip,
        take,
        relationTypeAlias = '',
    }: {
        childId: number,
        skip?: number,
        take?: number,
        relationTypeAlias?: string,
    }): CancelablePromise<PagedRelation> {
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
