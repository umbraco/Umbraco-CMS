/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DocumentTypeItemResponseModel } from '../models/DocumentTypeItemResponseModel';
import type { DocumentTypeResponseModel } from '../models/DocumentTypeResponseModel';
import type { PagedDocumentTypeTreeItemResponseModel } from '../models/PagedDocumentTypeTreeItemResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DocumentTypeResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDocumentTypeById({
        id,
    }: {
        id: string,
    }): CancelablePromise<DocumentTypeResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document-type/{id}',
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
    public static getDocumentTypeItem({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<DocumentTypeItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document-type/item',
            query: {
                'id': id,
            },
        });
    }

    /**
     * @returns PagedDocumentTypeTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeDocumentTypeChildren({
        parentId,
        skip,
        take = 100,
        foldersOnly = false,
    }: {
        parentId?: string,
        skip?: number,
        take?: number,
        foldersOnly?: boolean,
    }): CancelablePromise<PagedDocumentTypeTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document-type/children',
            query: {
                'parentId': parentId,
                'skip': skip,
                'take': take,
                'foldersOnly': foldersOnly,
            },
        });
    }

    /**
     * @returns PagedDocumentTypeTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeDocumentTypeRoot({
        skip,
        take = 100,
        foldersOnly = false,
    }: {
        skip?: number,
        take?: number,
        foldersOnly?: boolean,
    }): CancelablePromise<PagedDocumentTypeTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document-type/root',
            query: {
                'skip': skip,
                'take': take,
                'foldersOnly': foldersOnly,
            },
        });
    }

}
