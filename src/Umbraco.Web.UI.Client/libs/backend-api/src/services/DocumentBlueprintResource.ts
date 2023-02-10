/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DocumentBlueprintTreeItemModel } from '../models/DocumentBlueprintTreeItemModel';
import type { PagedDocumentBlueprintTreeItemModel } from '../models/PagedDocumentBlueprintTreeItemModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DocumentBlueprintResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTreeDocumentBlueprintItem({
        key,
    }: {
        key?: Array<string>,
    }): CancelablePromise<Array<DocumentBlueprintTreeItemModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document-blueprint/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedDocumentBlueprintTreeItemModel Success
     * @throws ApiError
     */
    public static getTreeDocumentBlueprintRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedDocumentBlueprintTreeItemModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document-blueprint/root',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
