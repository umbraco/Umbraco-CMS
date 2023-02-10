/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DocumentTypeTreeItemModel } from '../models/DocumentTypeTreeItemModel';
import type { PagedDocumentTypeTreeItemModel } from '../models/PagedDocumentTypeTreeItemModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DocumentTypeResource {

    /**
     * @returns PagedDocumentTypeTreeItemModel Success
     * @throws ApiError
     */
    public static getTreeDocumentTypeChildren({
        parentKey,
        skip,
        take = 100,
        foldersOnly = false,
    }: {
        parentKey?: string,
        skip?: number,
        take?: number,
        foldersOnly?: boolean,
    }): CancelablePromise<PagedDocumentTypeTreeItemModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document-type/children',
            query: {
                'parentKey': parentKey,
                'skip': skip,
                'take': take,
                'foldersOnly': foldersOnly,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTreeDocumentTypeItem({
        key,
    }: {
        key?: Array<string>,
    }): CancelablePromise<Array<DocumentTypeTreeItemModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document-type/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedDocumentTypeTreeItemModel Success
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
    }): CancelablePromise<PagedDocumentTypeTreeItemModel> {
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
