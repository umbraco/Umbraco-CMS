/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateRelationTypeRequestModel } from '../models/CreateRelationTypeRequestModel';
import type { PagedNamedEntityTreeItemResponseModel } from '../models/PagedNamedEntityTreeItemResponseModel';
import type { RelationTypeItemResponseModel } from '../models/RelationTypeItemResponseModel';
import type { RelationTypeResponseModel } from '../models/RelationTypeResponseModel';
import type { UpdateRelationTypeRequestModel } from '../models/UpdateRelationTypeRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class RelationTypeResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getItemRelationType({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<RelationTypeItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/item/relation-type',
            query: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postRelationType({
        requestBody,
    }: {
        requestBody?: CreateRelationTypeRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/relation-type',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Generated-Resource',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getRelationTypeById({
        id,
    }: {
        id: string,
    }): CancelablePromise<RelationTypeResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/relation-type/{id}',
            path: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static deleteRelationTypeById({
        id,
    }: {
        id: string,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/relation-type/{id}',
            path: {
                'id': id,
            },
            responseHeader: 'Umb-Notifications',
            errors: {
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putRelationTypeById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateRelationTypeRequestModel,
    }): CancelablePromise<RelationTypeResponseModel> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/relation-type/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns PagedNamedEntityTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeRelationTypeRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedNamedEntityTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/relation-type/root',
            query: {
                'skip': skip,
                'take': take,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

}
