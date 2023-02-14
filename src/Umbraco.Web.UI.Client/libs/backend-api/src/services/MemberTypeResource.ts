/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ContentTreeItemModel } from '../models/ContentTreeItemModel';
import type { DocumentBlueprintTreeItemModel } from '../models/DocumentBlueprintTreeItemModel';
import type { DocumentTreeItemModel } from '../models/DocumentTreeItemModel';
import type { DocumentTypeTreeItemModel } from '../models/DocumentTypeTreeItemModel';
import type { EntityTreeItemModel } from '../models/EntityTreeItemModel';
import type { FolderTreeItemModel } from '../models/FolderTreeItemModel';
import type { PagedEntityTreeItemModel } from '../models/PagedEntityTreeItemModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MemberTypeResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTreeMemberTypeItem({
        key,
    }: {
        key?: Array<string>,
    }): CancelablePromise<Array<(EntityTreeItemModel | ContentTreeItemModel | DocumentBlueprintTreeItemModel | DocumentTreeItemModel | DocumentTypeTreeItemModel | FolderTreeItemModel)>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/member-type/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItemModel Success
     * @throws ApiError
     */
    public static getTreeMemberTypeRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedEntityTreeItemModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/member-type/root',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
