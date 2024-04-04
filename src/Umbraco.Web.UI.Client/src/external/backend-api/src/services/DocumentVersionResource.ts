/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DocumentVersionResponseModel } from '../models/DocumentVersionResponseModel';
import type { PagedDocumentVersionItemResponseModel } from '../models/PagedDocumentVersionItemResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DocumentVersionResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDocumentVersion({
        documentId,
        culture,
        skip,
        take = 100,
    }: {
        documentId: string,
        culture?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedDocumentVersionItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document-version',
            query: {
                'documentId': documentId,
                'culture': culture,
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
    public static getDocumentVersionById({
        id,
    }: {
        id: string,
    }): CancelablePromise<DocumentVersionResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document-version/{id}',
            path: {
                'id': id,
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
    public static putDocumentVersionByIdPreventCleanup({
        id,
        preventCleanup,
    }: {
        id: string,
        preventCleanup?: boolean,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document-version/{id}/prevent-cleanup',
            path: {
                'id': id,
            },
            query: {
                'preventCleanup': preventCleanup,
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
    public static postDocumentVersionByIdRollback({
        id,
        culture,
    }: {
        id: string,
        culture?: string,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/document-version/{id}/rollback',
            path: {
                'id': id,
            },
            query: {
                'culture': culture,
            },
            responseHeader: 'Umb-Notifications',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

}
