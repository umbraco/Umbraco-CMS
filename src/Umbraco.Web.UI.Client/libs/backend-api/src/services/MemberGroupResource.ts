/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ContentTreeItemResponseModel } from '../models/ContentTreeItemResponseModel';
import type { DocumentBlueprintTreeItemResponseModel } from '../models/DocumentBlueprintTreeItemResponseModel';
import type { DocumentTreeItemResponseModel } from '../models/DocumentTreeItemResponseModel';
import type { DocumentTypeTreeItemResponseModel } from '../models/DocumentTypeTreeItemResponseModel';
import type { EntityTreeItemResponseModel } from '../models/EntityTreeItemResponseModel';
import type { FolderTreeItemResponseModel } from '../models/FolderTreeItemResponseModel';
import type { PagedEntityTreeItemResponseModel } from '../models/PagedEntityTreeItemResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MemberGroupResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTreeMemberGroupItem({
key,
}: {
key?: Array<string>,
}): CancelablePromise<Array<(EntityTreeItemResponseModel | ContentTreeItemResponseModel | DocumentBlueprintTreeItemResponseModel | DocumentTreeItemResponseModel | DocumentTypeTreeItemResponseModel | FolderTreeItemResponseModel)>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/member-group/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeMemberGroupRoot({
skip,
take = 100,
}: {
skip?: number,
take?: number,
}): CancelablePromise<PagedEntityTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/member-group/root',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
