/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateDocumentRequestModel } from '../models/CreateDocumentRequestModel';
import type { DocumentResponseModel } from '../models/DocumentResponseModel';
import type { DocumentTreeItemResponseModel } from '../models/DocumentTreeItemResponseModel';
import type { PagedDocumentTreeItemResponseModel } from '../models/PagedDocumentTreeItemResponseModel';
import type { PagedRecycleBinItemResponseModel } from '../models/PagedRecycleBinItemResponseModel';
import type { UpdateDocumentRequestModel } from '../models/UpdateDocumentRequestModel';
import type { UpdateDomainsRequestModel } from '../models/UpdateDomainsRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DocumentResource {

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postDocument({
requestBody,
}: {
requestBody?: CreateDocumentRequestModel,
}): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/document',
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
    public static getDocumentByKey({
key,
}: {
key: string,
}): CancelablePromise<DocumentResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document/{key}',
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
    public static deleteDocumentByKey({
key,
}: {
key: string,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/document/{key}',
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
    public static putDocumentByKey({
key,
requestBody,
}: {
key: string,
requestBody?: UpdateDocumentRequestModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document/{key}',
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
    public static getDocumentByKeyDomains({
key,
}: {
key: string,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document/{key}/domains',
            path: {
                'key': key,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putDocumentByKeyDomains({
key,
requestBody,
}: {
key: string,
requestBody?: UpdateDomainsRequestModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document/{key}/domains',
            path: {
                'key': key,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns PagedRecycleBinItemResponseModel Success
     * @throws ApiError
     */
    public static getRecycleBinDocumentChildren({
parentKey,
skip,
take = 100,
}: {
parentKey?: string,
skip?: number,
take?: number,
}): CancelablePromise<PagedRecycleBinItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/recycle-bin/document/children',
            query: {
                'parentKey': parentKey,
                'skip': skip,
                'take': take,
            },
            errors: {
                401: `Unauthorized`,
            },
        });
    }

    /**
     * @returns PagedRecycleBinItemResponseModel Success
     * @throws ApiError
     */
    public static getRecycleBinDocumentRoot({
skip,
take = 100,
}: {
skip?: number,
take?: number,
}): CancelablePromise<PagedRecycleBinItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/recycle-bin/document/root',
            query: {
                'skip': skip,
                'take': take,
            },
            errors: {
                401: `Unauthorized`,
            },
        });
    }

    /**
     * @returns PagedDocumentTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeDocumentChildren({
parentKey,
skip,
take = 100,
dataTypeKey,
culture,
}: {
parentKey?: string,
skip?: number,
take?: number,
dataTypeKey?: string,
culture?: string,
}): CancelablePromise<PagedDocumentTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document/children',
            query: {
                'parentKey': parentKey,
                'skip': skip,
                'take': take,
                'dataTypeKey': dataTypeKey,
                'culture': culture,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getTreeDocumentItem({
key,
dataTypeKey,
culture,
}: {
key?: Array<string>,
dataTypeKey?: string,
culture?: string,
}): CancelablePromise<Array<DocumentTreeItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document/item',
            query: {
                'key': key,
                'dataTypeKey': dataTypeKey,
                'culture': culture,
            },
        });
    }

    /**
     * @returns PagedDocumentTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeDocumentRoot({
skip,
take = 100,
dataTypeKey,
culture,
}: {
skip?: number,
take?: number,
dataTypeKey?: string,
culture?: string,
}): CancelablePromise<PagedDocumentTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document/root',
            query: {
                'skip': skip,
                'take': take,
                'dataTypeKey': dataTypeKey,
                'culture': culture,
            },
        });
    }

}
