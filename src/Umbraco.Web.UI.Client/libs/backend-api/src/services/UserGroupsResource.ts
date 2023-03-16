/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedUserGroupPresentationModel } from '../models/PagedUserGroupPresentationModel';
import type { SaveUserGroupRequestModel } from '../models/SaveUserGroupRequestModel';
import type { UpdateUserGroupRequestModel } from '../models/UpdateUserGroupRequestModel';
import type { UserGroupPresentationModel } from '../models/UserGroupPresentationModel';

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
requestBody?: SaveUserGroupRequestModel,
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
     * @returns PagedUserGroupPresentationModel Success
     * @throws ApiError
     */
    public static getUserGroups({
skip,
take = 100,
}: {
skip?: number,
take?: number,
}): CancelablePromise<PagedUserGroupPresentationModel> {
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
}): CancelablePromise<UserGroupPresentationModel> {
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
requestBody?: UpdateUserGroupRequestModel,
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
