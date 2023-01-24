/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { EntityTreeItem } from '../models/EntityTreeItem';
import type { PagedEntityTreeItem } from '../models/PagedEntityTreeItem';
import type { Template } from '../models/Template';
import type { TemplateCreateModel } from '../models/TemplateCreateModel';
import type { TemplateQueryExecuteModel } from '../models/TemplateQueryExecuteModel';
import type { TemplateQueryResult } from '../models/TemplateQueryResult';
import type { TemplateQuerySettings } from '../models/TemplateQuerySettings';
import type { TemplateScaffold } from '../models/TemplateScaffold';
import type { TemplateUpdateModel } from '../models/TemplateUpdateModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class TemplateResource {

    /**
     * @returns any Created
     * @throws ApiError
     */
    public static postTemplate({
        requestBody,
    }: {
        requestBody?: TemplateCreateModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/template',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns Template Success
     * @throws ApiError
     */
    public static getTemplateByKey({
        key,
    }: {
        key: string,
    }): CancelablePromise<Template> {
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
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns TemplateQueryResult Success
     * @throws ApiError
     */
    public static postTemplateQueryExecute({
        requestBody,
    }: {
        requestBody?: TemplateQueryExecuteModel,
    }): CancelablePromise<TemplateQueryResult> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/template/query/execute',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns TemplateQuerySettings Success
     * @throws ApiError
     */
    public static getTemplateQuerySettings(): CancelablePromise<TemplateQuerySettings> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/template/query/settings',
        });
    }

    /**
     * @returns TemplateScaffold Success
     * @throws ApiError
     */
    public static getTemplateScaffold({
        masterTemplateAlias,
    }: {
        masterTemplateAlias?: string,
    }): CancelablePromise<TemplateScaffold> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/template/scaffold',
            query: {
                'masterTemplateAlias': masterTemplateAlias,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItem Success
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
    }): CancelablePromise<PagedEntityTreeItem> {
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
     * @returns EntityTreeItem Success
     * @throws ApiError
     */
    public static getTreeTemplateItem({
        key,
    }: {
        key?: Array<string>,
    }): CancelablePromise<Array<EntityTreeItem>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/template/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItem Success
     * @throws ApiError
     */
    public static getTreeTemplateRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedEntityTreeItem> {
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
