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
import type { UserOrderModel } from '../models/UserOrderModel';
import type { UserResponseModel } from '../models/UserResponseModel';
import type { UserStateModel } from '../models/UserStateModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class UsersResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postUsers({
        requestBody,
    }: {
        requestBody?: (CreateUserRequestModel | InviteUserRequestModel),
    }): CancelablePromise<CreateUserResponseModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/users',
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
    public static getUsers({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedUserResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/users',
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
    public static getUsersById({
        id,
    }: {
        id: string,
    }): CancelablePromise<UserResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/users/{id}',
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
    public static deleteUsersById({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/users/{id}',
            path: {
                'id': id,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putUsersById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateUserRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/users/{id}',
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
    public static deleteUsersAvatarById({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/users/avatar/{id}',
            path: {
                'id': id,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postUsersAvatarById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: SetAvatarRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/users/avatar/{id}',
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
    public static postUsersChangePasswordById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: ChangePasswordUserRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/users/change-password/{id}',
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
    public static postUsersDisable({
        requestBody,
    }: {
        requestBody?: DisableUserRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/users/disable',
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
    public static postUsersEnable({
        requestBody,
    }: {
        requestBody?: EnableUserRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/users/enable',
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
    public static getUsersFilter({
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
            url: '/umbraco/management/api/v1/users/filter',
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
     * @returns any Success
     * @throws ApiError
     */
    public static postUsersInvite({
        requestBody,
    }: {
        requestBody?: InviteUserRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/users/invite',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postUsersSetUserGroups({
        requestBody,
    }: {
        requestBody?: UpdateUserGroupsOnUserRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/users/set-user-groups',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postUsersUnlock({
        requestBody,
    }: {
        requestBody?: UnlockUsersRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/users/unlock',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

}
