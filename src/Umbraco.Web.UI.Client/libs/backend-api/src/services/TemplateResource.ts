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
import type { TemplateCreateModel } from '../models/TemplateCreateModel';
import type { TemplateModel } from '../models/TemplateModel';
import type { TemplateQueryExecuteModel } from '../models/TemplateQueryExecuteModel';
import type { TemplateQueryResultModel } from '../models/TemplateQueryResultModel';
import type { TemplateQuerySettingsModel } from '../models/TemplateQuerySettingsModel';
import type { TemplateScaffoldModel } from '../models/TemplateScaffoldModel';
import type { TemplateUpdateModel } from '../models/TemplateUpdateModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class TemplateResource {

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postTemplate({
        requestBody,
    }: {
        requestBody?: TemplateCreateModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/template',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Location',
            errors: {
                400: `Bad Request`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTemplateByKey({
        key,
    }: {
        key: string,
    }): CancelablePromise<TemplateModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/template/{key}',
            path: {
                'key': key,
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
    public static deleteTemplateByKey({
        key,
    }: {
        key: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/template/{key}',
            path: {
                'key': key,
            },
            errors: {
                400: `Bad Request`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putTemplateByKey({
        key,
        requestBody,
    }: {
        key: string,
        requestBody?: TemplateUpdateModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/template/{key}',
            path: {
                'key': key,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postTemplateQueryExecute({
        requestBody,
    }: {
        requestBody?: TemplateQueryExecuteModel,
    }): CancelablePromise<TemplateQueryResultModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/template/query/execute',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTemplateQuerySettings(): CancelablePromise<TemplateQuerySettingsModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/template/query/settings',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTemplateScaffold(): CancelablePromise<TemplateScaffoldModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/template/scaffold',
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItemModel Success
     * @throws ApiError
     */
    public static getTreeTemplateChildren({
        parentKey,
        skip,
        take = 100,
    }: {
        parentKey?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedEntityTreeItemModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/template/children',
            query: {
                'parentKey': parentKey,
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTreeTemplateItem({
        key,
    }: {
        key?: Array<string>,
    }): CancelablePromise<Array<(EntityTreeItemModel | ContentTreeItemModel | DocumentBlueprintTreeItemModel | DocumentTreeItemModel | DocumentTypeTreeItemModel | FolderTreeItemModel)>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/template/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItemModel Success
     * @throws ApiError
     */
    public static getTreeTemplateRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedEntityTreeItemModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/template/root',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
