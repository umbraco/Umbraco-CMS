/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedUserGroupModel } from '../models/PagedUserGroupModel';
import type { UserGroupModel } from '../models/UserGroupModel';
import type { UserGroupSaveModel } from '../models/UserGroupSaveModel';
import type { UserGroupUpdateModel } from '../models/UserGroupUpdateModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class UserGroupsResource {

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postUserGroups({
        requestBody,
    }: {
        requestBody?: UserGroupSaveModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user-groups',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Location',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns PagedUserGroupModel Success
     * @throws ApiError
     */
    public static getUserGroups({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedUserGroupModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user-groups',
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
    public static getUserGroupsByKey({
        key,
    }: {
        key: string,
    }): CancelablePromise<UserGroupModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user-groups/{key}',
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
    public static deleteUserGroupsByKey({
        key,
    }: {
        key: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/user-groups/{key}',
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
    public static putUserGroupsByKey({
        key,
        requestBody,
    }: {
        key: string,
        requestBody?: UserGroupUpdateModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/user-groups/{key}',
            path: {
                'key': key,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                404: `Not Found`,
            },
        });
    }

}
