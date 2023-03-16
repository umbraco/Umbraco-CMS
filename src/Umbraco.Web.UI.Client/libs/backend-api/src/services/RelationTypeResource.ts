/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateRelationTypeRequestModel } from '../models/CreateRelationTypeRequestModel';
import type { DocumentTypeTreeItemResponseModel } from '../models/DocumentTypeTreeItemResponseModel';
import type { FolderTreeItemResponseModel } from '../models/FolderTreeItemResponseModel';
import type { PagedEntityTreeItemResponseModel } from '../models/PagedEntityTreeItemResponseModel';
import type { RelationTypeResponseModel } from '../models/RelationTypeResponseModel';
import type { UpdateRelationTypeRequestModel } from '../models/UpdateRelationTypeRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class RelationTypeResource {

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
            responseHeader: 'Location',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getRelationTypeByKey({
key,
}: {
key: string,
}): CancelablePromise<RelationTypeResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/relation-type/{key}',
            path: {
                'key': key,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteRelationTypeByKey({
key,
}: {
key: string,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/relation-type/{key}',
            path: {
                'key': key,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putRelationTypeByKey({
key,
requestBody,
}: {
key: string,
requestBody?: UpdateRelationTypeRequestModel,
}): CancelablePromise<RelationTypeResponseModel> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/relation-type/{key}',
            path: {
                'key': key,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTreeRelationTypeItem({
key,
}: {
key?: Array<string>,
}): CancelablePromise<Array<(FolderTreeItemResponseModel | DocumentTypeTreeItemResponseModel)>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/relation-type/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeRelationTypeRoot({
skip,
take = 100,
}: {
skip?: number,
take?: number,
}): CancelablePromise<PagedEntityTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/relation-type/root',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
