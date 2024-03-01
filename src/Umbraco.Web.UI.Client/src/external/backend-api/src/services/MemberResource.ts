/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateMemberRequestModel } from '../models/CreateMemberRequestModel';
import type { DirectionModel } from '../models/DirectionModel';
import type { MemberItemResponseModel } from '../models/MemberItemResponseModel';
import type { MemberResponseModel } from '../models/MemberResponseModel';
import type { PagedMemberResponseModel } from '../models/PagedMemberResponseModel';
import type { UpdateMemberRequestModel } from '../models/UpdateMemberRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MemberResource {

    /**
     * @returns PagedMemberResponseModel Success
     * @throws ApiError
     */
    public static getFilterMember({
        memberTypeId,
        orderBy = 'username',
        orderDirection,
        filter,
        skip,
        take = 100,
    }: {
        memberTypeId?: string,
        orderBy?: string,
        orderDirection?: DirectionModel,
        filter?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedMemberResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/filter/member',
            query: {
                'memberTypeId': memberTypeId,
                'orderBy': orderBy,
                'orderDirection': orderDirection,
                'filter': filter,
                'skip': skip,
                'take': take,
            },
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
    public static getItemMember({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<MemberItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/item/member',
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
    public static postMember({
        requestBody,
    }: {
        requestBody?: CreateMemberRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/member',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Generated-Resource',
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
    public static getMemberById({
        id,
    }: {
        id: string,
    }): CancelablePromise<MemberResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/member/{id}',
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
    public static deleteMemberById({
        id,
    }: {
        id: string,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/member/{id}',
            path: {
                'id': id,
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
     * @returns string Success
     * @throws ApiError
     */
    public static putMemberById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateMemberRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/member/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
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
    public static putMemberByIdValidate({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateMemberRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/member/{id}/validate',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns PagedMemberResponseModel Success
     * @throws ApiError
     */
    public static getMemberFilter({
        memberTypeId,
        orderBy = 'username',
        orderDirection,
        filter,
        skip,
        take = 100,
    }: {
        memberTypeId?: string,
        orderBy?: string,
        orderDirection?: DirectionModel,
        filter?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedMemberResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/member/filter',
            query: {
                'memberTypeId': memberTypeId,
                'orderBy': orderBy,
                'orderDirection': orderDirection,
                'filter': filter,
                'skip': skip,
                'take': take,
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
    public static postMemberValidate({
        requestBody,
    }: {
        requestBody?: CreateMemberRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/member/validate',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Notifications',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

}
