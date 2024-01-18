/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AvailableDocumentTypeCompositionResponseModel } from '../models/AvailableDocumentTypeCompositionResponseModel';
import type { CopyDocumentTypeRequestModel } from '../models/CopyDocumentTypeRequestModel';
import type { CreateDocumentTypeRequestModel } from '../models/CreateDocumentTypeRequestModel';
import type { CreateFolderRequestModel } from '../models/CreateFolderRequestModel';
import type { DocumentTypeCompositionRequestModel } from '../models/DocumentTypeCompositionRequestModel';
import type { DocumentTypeCompositionResponseModel } from '../models/DocumentTypeCompositionResponseModel';
import type { DocumentTypeItemResponseModel } from '../models/DocumentTypeItemResponseModel';
import type { DocumentTypeResponseModel } from '../models/DocumentTypeResponseModel';
import type { FolderResponseModel } from '../models/FolderResponseModel';
import type { MoveDocumentTypeRequestModel } from '../models/MoveDocumentTypeRequestModel';
import type { PagedDocumentTypeTreeItemResponseModel } from '../models/PagedDocumentTypeTreeItemResponseModel';
import type { UpdateDocumentTypeRequestModel } from '../models/UpdateDocumentTypeRequestModel';
import type { UpdateFolderResponseModel } from '../models/UpdateFolderResponseModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DocumentTypeResource {

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postDocumentType({
requestBody,
}: {
requestBody?: CreateDocumentTypeRequestModel,
}): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/document-type',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Location',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDocumentTypeById({
id,
}: {
id: string,
}): CancelablePromise<DocumentTypeResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document-type/{id}',
            path: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteDocumentTypeById({
id,
}: {
id: string,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/document-type/{id}',
            path: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putDocumentTypeById({
id,
requestBody,
}: {
id: string,
requestBody?: UpdateDocumentTypeRequestModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document-type/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDocumentTypeByIdCompositionReferences({
id,
}: {
id: string,
}): CancelablePromise<Array<DocumentTypeCompositionResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document-type/{id}/composition-references',
            path: {
                'id': id,
            },
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postDocumentTypeByIdCopy({
id,
requestBody,
}: {
id: string,
requestBody?: CopyDocumentTypeRequestModel,
}): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/document-type/{id}/copy',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Location',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putDocumentTypeByIdMove({
id,
requestBody,
}: {
id: string,
requestBody?: MoveDocumentTypeRequestModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document-type/{id}/move',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postDocumentTypeAvailableCompositions({
requestBody,
}: {
requestBody?: DocumentTypeCompositionRequestModel,
}): CancelablePromise<Array<AvailableDocumentTypeCompositionResponseModel>> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/document-type/available-compositions',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postDocumentTypeFolder({
requestBody,
}: {
requestBody?: CreateFolderRequestModel,
}): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/document-type/folder',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Location',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDocumentTypeFolderById({
id,
}: {
id: string,
}): CancelablePromise<FolderResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document-type/folder/{id}',
            path: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteDocumentTypeFolderById({
id,
}: {
id: string,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/document-type/folder/{id}',
            path: {
                'id': id,
            },
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putDocumentTypeFolderById({
id,
requestBody,
}: {
id: string,
requestBody?: UpdateFolderResponseModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document-type/folder/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDocumentTypeItem({
id,
}: {
id?: Array<string>,
}): CancelablePromise<Array<DocumentTypeItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document-type/item',
            query: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns PagedDocumentTypeTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeDocumentTypeChildren({
parentId,
skip,
take = 100,
foldersOnly = false,
}: {
parentId?: string,
skip?: number,
take?: number,
foldersOnly?: boolean,
}): CancelablePromise<PagedDocumentTypeTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document-type/children',
            query: {
                'parentId': parentId,
                'skip': skip,
                'take': take,
                'foldersOnly': foldersOnly,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns PagedDocumentTypeTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeDocumentTypeRoot({
skip,
take = 100,
foldersOnly = false,
}: {
skip?: number,
take?: number,
foldersOnly?: boolean,
}): CancelablePromise<PagedDocumentTypeTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document-type/root',
            query: {
                'skip': skip,
                'take': take,
                'foldersOnly': foldersOnly,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

}
