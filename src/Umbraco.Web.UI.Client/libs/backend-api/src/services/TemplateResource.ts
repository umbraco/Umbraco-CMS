/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ContentTreeItemResponseModel } from '../models/ContentTreeItemResponseModel';
import type { CreateTemplateRequestModel } from '../models/CreateTemplateRequestModel';
import type { DocumentBlueprintTreeItemResponseModel } from '../models/DocumentBlueprintTreeItemResponseModel';
import type { DocumentTreeItemResponseModel } from '../models/DocumentTreeItemResponseModel';
import type { DocumentTypeTreeItemResponseModel } from '../models/DocumentTypeTreeItemResponseModel';
import type { EntityTreeItemResponseModel } from '../models/EntityTreeItemResponseModel';
import type { FolderTreeItemResponseModel } from '../models/FolderTreeItemResponseModel';
import type { PagedEntityTreeItemResponseModel } from '../models/PagedEntityTreeItemResponseModel';
import type { TemplateQueryExecuteModel } from '../models/TemplateQueryExecuteModel';
import type { TemplateQueryResultResponseModel } from '../models/TemplateQueryResultResponseModel';
import type { TemplateQuerySettingsResponseModel } from '../models/TemplateQuerySettingsResponseModel';
import type { TemplateResponseModel } from '../models/TemplateResponseModel';
import type { TemplateScaffoldResponseModel } from '../models/TemplateScaffoldResponseModel';
import type { UpdateTemplateRequestModel } from '../models/UpdateTemplateRequestModel';

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
requestBody?: CreateTemplateRequestModel,
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
}): CancelablePromise<TemplateResponseModel> {
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
requestBody?: UpdateTemplateRequestModel,
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
}): CancelablePromise<TemplateQueryResultResponseModel> {
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
    public static getTemplateQuerySettings(): CancelablePromise<TemplateQuerySettingsResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/template/query/settings',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTemplateScaffold(): CancelablePromise<TemplateScaffoldResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/template/scaffold',
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItemResponseModel Success
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
}): CancelablePromise<PagedEntityTreeItemResponseModel> {
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
}): CancelablePromise<Array<(EntityTreeItemResponseModel | ContentTreeItemResponseModel | DocumentBlueprintTreeItemResponseModel | DocumentTreeItemResponseModel | DocumentTypeTreeItemResponseModel | FolderTreeItemResponseModel)>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/template/item',
            query: {
                'key': key,
            },
        });
    }

    /**
     * @returns PagedEntityTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeTemplateRoot({
skip,
take = 100,
}: {
skip?: number,
take?: number,
}): CancelablePromise<PagedEntityTreeItemResponseModel> {
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
