/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ChangePasswordUserRequestModel } from '../models/ChangePasswordUserRequestModel';
import type { CreateUserRequestModel } from '../models/CreateUserRequestModel';
import type { CreateUserResponseModel } from '../models/CreateUserResponseModel';
import type { DirectionModel } from '../models/DirectionModel';
import type { DisableUserRequestModel } from '../models/DisableUserRequestModel';
import type { EnableUserRequestModel } from '../models/EnableUserRequestModel';
import type { InviteUserRequestModel } from '../models/InviteUserRequestModel';
import type { PagedUserResponseModel } from '../models/PagedUserResponseModel';
import type { SetAvatarRequestModel } from '../models/SetAvatarRequestModel';
import type { UnlockUsersRequestModel } from '../models/UnlockUsersRequestModel';
import type { UpdateUserGroupsOnUserRequestModel } from '../models/UpdateUserGroupsOnUserRequestModel';
import type { UpdateUserRequestModel } from '../models/UpdateUserRequestModel';
import type { UserItemResponseModel } from '../models/UserItemResponseModel';
import type { UserOrderModel } from '../models/UserOrderModel';
import type { UserResponseModel } from '../models/UserResponseModel';
import type { UserStateModel } from '../models/UserStateModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class UserResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postUser({
        requestBody,
    }: {
        requestBody?: (CreateUserRequestModel | InviteUserRequestModel),
    }): CancelablePromise<CreateUserResponseModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns PagedUserResponseModel Success
     * @throws ApiError
     */
    public static getUser({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedUserResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user',
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
    public static getUserById({
        id,
    }: {
        id: string,
    }): CancelablePromise<UserResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user/{id}',
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
    public static deleteUserById({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/user/{id}',
            path: {
                'id': id,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putUserById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateUserRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/user/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteUserAvatarById({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/user/avatar/{id}',
            path: {
                'id': id,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postUserAvatarById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: SetAvatarRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/avatar/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postUserChangePasswordById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: ChangePasswordUserRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/change-password/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postUserDisable({
        requestBody,
    }: {
        requestBody?: DisableUserRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/disable',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postUserEnable({
        requestBody,
    }: {
        requestBody?: EnableUserRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/enable',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getUserFilter({
        skip,
        take = 100,
        orderBy,
        orderDirection,
        userGroupIds,
        userStates,
        filter = '',
    }: {
        skip?: number,
        take?: number,
        orderBy?: UserOrderModel,
        orderDirection?: DirectionModel,
        userGroupIds?: Array<string>,
        userStates?: Array<UserStateModel>,
        filter?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user/filter',
            query: {
                'skip': skip,
                'take': take,
                'orderBy': orderBy,
                'orderDirection': orderDirection,
                'userGroupIds': userGroupIds,
                'userStates': userStates,
                'filter': filter,
            },
        });
    }

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postUserInvite({
        requestBody,
    }: {
        requestBody?: InviteUserRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/invite',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Location',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getUserItem({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<UserItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user/item',
            query: {
                'id': id,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postUserSetUserGroups({
        requestBody,
    }: {
        requestBody?: UpdateUserGroupsOnUserRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/set-user-groups',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postUserUnlock({
        requestBody,
    }: {
        requestBody?: UnlockUsersRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/unlock',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

}
