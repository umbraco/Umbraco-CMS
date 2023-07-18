/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateUserGroupRequestModel } from '../models/CreateUserGroupRequestModel';
import type { PagedUserGroupResponseModel } from '../models/PagedUserGroupResponseModel';
import type { UpdateUserGroupRequestModel } from '../models/UpdateUserGroupRequestModel';
import type { UserGroupItemResponseModel } from '../models/UserGroupItemResponseModel';
import type { UserGroupResponseModel } from '../models/UserGroupResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class UserGroupResource {

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postUserGroup({
        requestBody,
    }: {
        requestBody?: CreateUserGroupRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user-group',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Location',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns PagedUserGroupResponseModel Success
     * @throws ApiError
     */
    public static getUserGroup({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedUserGroupResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user-group',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getUserGroupById({
        id,
    }: {
        id: string,
    }): CancelablePromise<UserGroupResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user-group/{id}',
            path: {
                'id': id,
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
    public static deleteUserGroupById({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/user-group/{id}',
            path: {
                'id': id,
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
    public static putUserGroupById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateUserGroupRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/user-group/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getUserGroupItem({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<UserGroupItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user-group/item',
            query: {
                'id': id,
            },
        });
    }

}
