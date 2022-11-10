/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DocumentBlueprintTreeItem } from '../models/DocumentBlueprintTreeItem';
import type { PagedDocumentBlueprintTreeItem } from '../models/PagedDocumentBlueprintTreeItem';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DocumentBlueprintResource {

    /**
     * @returns DocumentBlueprintTreeItem Success
     * @throws ApiError
     */
    public static getTreeDocumentBlueprintItem({
        key,
    }: {
        key?: Array<string>,
    }): CancelablePromise<Array<DocumentBlueprintTreeItem>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document-blueprint/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedDocumentBlueprintTreeItem Success
     * @throws ApiError
     */
    public static getTreeDocumentBlueprintRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedDocumentBlueprintTreeItem> {
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
