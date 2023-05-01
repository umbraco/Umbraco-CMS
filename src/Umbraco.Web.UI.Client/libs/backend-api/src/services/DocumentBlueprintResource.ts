/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DocumentBlueprintResponseModel } from '../models/DocumentBlueprintResponseModel';
import type { PagedDocumentBlueprintTreeItemResponseModel } from '../models/PagedDocumentBlueprintTreeItemResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DocumentBlueprintResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDocumentBlueprintItem({
id,
}: {
id?: Array<string>,
}): CancelablePromise<Array<DocumentBlueprintResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document-blueprint/item',
            query: {
                'id': id,
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
