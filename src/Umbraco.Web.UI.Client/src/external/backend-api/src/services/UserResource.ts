/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ChangePasswordUserRequestModel } from '../models/ChangePasswordUserRequestModel';
import type { CreateInitialPasswordUserRequestModel } from '../models/CreateInitialPasswordUserRequestModel';
import type { CreateUserRequestModel } from '../models/CreateUserRequestModel';
import type { CreateUserResponseModel } from '../models/CreateUserResponseModel';
import type { CurrentUserResponseModel } from '../models/CurrentUserResponseModel';
import type { CurrenUserConfigurationResponseModel } from '../models/CurrenUserConfigurationResponseModel';
import type { DeleteUsersRequestModel } from '../models/DeleteUsersRequestModel';
import type { DirectionModel } from '../models/DirectionModel';
import type { DisableUserRequestModel } from '../models/DisableUserRequestModel';
import type { EnableTwoFactorRequestModel } from '../models/EnableTwoFactorRequestModel';
import type { EnableUserRequestModel } from '../models/EnableUserRequestModel';
import type { InviteUserRequestModel } from '../models/InviteUserRequestModel';
import type { LinkedLoginsRequestModel } from '../models/LinkedLoginsRequestModel';
import type { NoopSetupTwoFactorModel } from '../models/NoopSetupTwoFactorModel';
import type { PagedUserResponseModel } from '../models/PagedUserResponseModel';
import type { ResendInviteUserRequestModel } from '../models/ResendInviteUserRequestModel';
import type { SetAvatarRequestModel } from '../models/SetAvatarRequestModel';
import type { UnlockUsersRequestModel } from '../models/UnlockUsersRequestModel';
import type { UpdateUserGroupsOnUserRequestModel } from '../models/UpdateUserGroupsOnUserRequestModel';
import type { UpdateUserRequestModel } from '../models/UpdateUserRequestModel';
import type { UserConfigurationResponseModel } from '../models/UserConfigurationResponseModel';
import type { UserItemResponseModel } from '../models/UserItemResponseModel';
import type { UserOrderModel } from '../models/UserOrderModel';
import type { UserPermissionsResponseModel } from '../models/UserPermissionsResponseModel';
import type { UserResponseModel } from '../models/UserResponseModel';
import type { UserStateModel } from '../models/UserStateModel';
import type { UserTwoFactorProviderModel } from '../models/UserTwoFactorProviderModel';
import type { VerifyInviteUserRequestModel } from '../models/VerifyInviteUserRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class UserResource {

    /**
     * @returns PagedUserResponseModel Success
     * @throws ApiError
     */
    public static getFilterUser({
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
    }): CancelablePromise<PagedUserResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/filter/user',
            query: {
                'skip': skip,
                'take': take,
                'orderBy': orderBy,
                'orderDirection': orderDirection,
                'userGroupIds': userGroupIds,
                'userStates': userStates,
                'filter': filter,
            },
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getItemUser({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<UserItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/item/user',
            query: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

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
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static deleteUser({
        requestBody,
    }: {
        requestBody?: DeleteUsersRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/user',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
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
            errors: {
                401: `The resource is protected and requires an authentication token`,
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
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static deleteUserById({
        id,
    }: {
        id: string,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/user/{id}',
            path: {
                'id': id,
            },
            responseHeader: 'Umb-Notifications',
            errors: {
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static putUserById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateUserRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/user/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getUserById2Fa({
        id,
    }: {
        id: string,
    }): CancelablePromise<Array<UserTwoFactorProviderModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user/{id}/2fa',
            path: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static deleteUserById2FaByProviderName({
        id,
        providerName,
    }: {
        id: string,
        providerName: string,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/user/{id}/2fa/{providerName}',
            path: {
                'id': id,
                'providerName': providerName,
            },
            responseHeader: 'Umb-Notifications',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static deleteUserAvatarById({
        id,
    }: {
        id: string,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/user/avatar/{id}',
            path: {
                'id': id,
            },
            responseHeader: 'Umb-Notifications',
            errors: {
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static postUserAvatarById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: SetAvatarRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/avatar/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static postUserChangePasswordById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: ChangePasswordUserRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/change-password/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getUserConfiguration(): CancelablePromise<UserConfigurationResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user/configuration',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getUserCurrent(): CancelablePromise<CurrentUserResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user/current',
            errors: {
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getUserCurrent2Fa(): CancelablePromise<Array<UserTwoFactorProviderModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user/current/2fa',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static deleteUserCurrent2FaByProviderName({
        providerName,
        code,
    }: {
        providerName: string,
        code?: string,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/user/current/2fa/{providerName}',
            path: {
                'providerName': providerName,
            },
            query: {
                'code': code,
            },
            responseHeader: 'Umb-Notifications',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postUserCurrent2FaByProviderName({
        providerName,
        requestBody,
    }: {
        providerName: string,
        requestBody?: EnableTwoFactorRequestModel,
    }): CancelablePromise<NoopSetupTwoFactorModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/current/2fa/{providerName}',
            path: {
                'providerName': providerName,
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
     * @returns any Success
     * @throws ApiError
     */
    public static getUserCurrent2FaByProviderName({
        providerName,
    }: {
        providerName: string,
    }): CancelablePromise<NoopSetupTwoFactorModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user/current/2fa/{providerName}',
            path: {
                'providerName': providerName,
            },
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static postUserCurrentAvatar({
        requestBody,
    }: {
        requestBody?: SetAvatarRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/current/avatar',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
            errors: {
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static postUserCurrentChangePassword({
        requestBody,
    }: {
        requestBody?: ChangePasswordUserRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/current/change-password',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getUserCurrentConfiguration(): CancelablePromise<CurrenUserConfigurationResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user/current/configuration',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getUserCurrentLogins(): CancelablePromise<LinkedLoginsRequestModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user/current/logins',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getUserCurrentPermissions({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<UserPermissionsResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user/current/permissions',
            query: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getUserCurrentPermissionsDocument({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<UserPermissionsResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user/current/permissions/document',
            query: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getUserCurrentPermissionsMedia({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<UserPermissionsResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/user/current/permissions/media',
            query: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static postUserDisable({
        requestBody,
    }: {
        requestBody?: DisableUserRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/disable',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static postUserEnable({
        requestBody,
    }: {
        requestBody?: EnableUserRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/enable',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
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
            responseHeader: 'Umb-Generated-Resource',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static postUserInviteCreatePassword({
        requestBody,
    }: {
        requestBody?: CreateInitialPasswordUserRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/invite/create-password',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
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
    public static postUserInviteResend({
        requestBody,
    }: {
        requestBody?: ResendInviteUserRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/invite/resend',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static postUserInviteVerify({
        requestBody,
    }: {
        requestBody?: (VerifyInviteUserRequestModel | CreateInitialPasswordUserRequestModel),
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/invite/verify',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
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
    public static postUserSetUserGroups({
        requestBody,
    }: {
        requestBody?: UpdateUserGroupsOnUserRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/set-user-groups',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
            errors: {
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
            },
        });
    }

    /**
     * @returns string Success
     * @throws ApiError
     */
    public static postUserUnlock({
        requestBody,
    }: {
        requestBody?: UnlockUsersRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/user/unlock',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
            },
        });
    }

}
