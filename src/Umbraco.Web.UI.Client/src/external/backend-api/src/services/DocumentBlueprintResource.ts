/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateDocumentBlueprintFromDocumentRequestModel } from '../models/CreateDocumentBlueprintFromDocumentRequestModel';
import type { CreateDocumentBlueprintRequestModel } from '../models/CreateDocumentBlueprintRequestModel';
import type { DocumentBlueprintItemResponseModel } from '../models/DocumentBlueprintItemResponseModel';
import type { DocumentBlueprintResponseModel } from '../models/DocumentBlueprintResponseModel';
import type { PagedDocumentBlueprintTreeItemResponseModel } from '../models/PagedDocumentBlueprintTreeItemResponseModel';
import type { UpdateDocumentBlueprintRequestModel } from '../models/UpdateDocumentBlueprintRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DocumentBlueprintResource {

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postDocumentBlueprint({
        requestBody,
    }: {
        requestBody?: CreateDocumentBlueprintRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/document-blueprint',
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
     * @returns DocumentBlueprintResponseModel Success
     * @throws ApiError
     */
    public static getDocumentBlueprintById({
        id,
    }: {
        id: string,
    }): CancelablePromise<DocumentBlueprintResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document-blueprint/{id}',
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
    public static deleteDocumentBlueprintById({
        id,
    }: {
        id: string,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/document-blueprint/{id}',
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
    public static putDocumentBlueprintById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateDocumentBlueprintRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document-blueprint/{id}',
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
     * @returns string Created
     * @throws ApiError
     */
    public static postDocumentBlueprintFromDocument({
        requestBody,
    }: {
        requestBody?: CreateDocumentBlueprintFromDocumentRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/document-blueprint/from-document',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Generated-Resource',
            errors: {
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns DocumentBlueprintItemResponseModel Success
     * @throws ApiError
     */
    public static getItemDocumentBlueprint({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<DocumentBlueprintItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/item/document-blueprint',
            query: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns PagedDocumentBlueprintTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeDocumentBlueprintRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedDocumentBlueprintTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document-blueprint/root',
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
