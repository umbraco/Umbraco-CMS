/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateMemberGroupRequestModel } from '../models/CreateMemberGroupRequestModel';
import type { MemberGroupItemResponseModel } from '../models/MemberGroupItemResponseModel';
import type { MemberGroupResponseModel } from '../models/MemberGroupResponseModel';
import type { PagedMemberGroupResponseModel } from '../models/PagedMemberGroupResponseModel';
import type { PagedNamedEntityTreeItemResponseModel } from '../models/PagedNamedEntityTreeItemResponseModel';
import type { UpdateMemberGroupRequestModel } from '../models/UpdateMemberGroupRequestModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class MemberGroupResource {
    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getItemMemberGroup({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<MemberGroupItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/item/member-group',
            query: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }
    /**
     * @returns PagedMemberGroupResponseModel Success
     * @throws ApiError
     */
    public static getMemberGroup({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedMemberGroupResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/member-group',
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
    public static postMemberGroup({
        requestBody,
    }: {
        requestBody?: CreateMemberGroupRequestModel,
    }): CancelablePromise<MemberGroupResponseModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/member-group',
            body: requestBody,
            mediaType: 'application/json',
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
    public static putMemberGroupById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateMemberGroupRequestModel,
    }): CancelablePromise<MemberGroupResponseModel> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/member-group/{id}',
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
     * @returns string Success
     * @throws ApiError
     */
    public static deleteMemberGroupByKey({
        key,
    }: {
        key: string,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/member-group/{key}',
            path: {
                'key': key,
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
     * @returns PagedNamedEntityTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeMemberGroupRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedNamedEntityTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/member-group/root',
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
