/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DocumentBlueprintTreeItemResponseModel } from '../models/DocumentBlueprintTreeItemResponseModel';
import type { PagedDocumentBlueprintTreeItemResponseModel } from '../models/PagedDocumentBlueprintTreeItemResponseModel';

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
}): CancelablePromise<Array<DocumentBlueprintTreeItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document-blueprint/item',
            query: {
                'key': key,
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
        });
    }

}
