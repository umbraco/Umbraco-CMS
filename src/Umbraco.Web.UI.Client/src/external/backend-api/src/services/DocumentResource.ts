/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CopyDocumentRequestModel } from '../models/CopyDocumentRequestModel';
import type { CreateDocumentRequestModel } from '../models/CreateDocumentRequestModel';
import type { DirectionModel } from '../models/DirectionModel';
import type { DocumentConfigurationResponseModel } from '../models/DocumentConfigurationResponseModel';
import type { DocumentItemResponseModel } from '../models/DocumentItemResponseModel';
import type { DocumentNotificationResponseModel } from '../models/DocumentNotificationResponseModel';
import type { DocumentResponseModel } from '../models/DocumentResponseModel';
import type { DomainsResponseModel } from '../models/DomainsResponseModel';
import type { MoveDocumentRequestModel } from '../models/MoveDocumentRequestModel';
import type { PagedDocumentCollectionResponseModel } from '../models/PagedDocumentCollectionResponseModel';
import type { PagedDocumentRecycleBinItemResponseModel } from '../models/PagedDocumentRecycleBinItemResponseModel';
import type { PagedDocumentTreeItemResponseModel } from '../models/PagedDocumentTreeItemResponseModel';
import type { PublicAccessRequestModel } from '../models/PublicAccessRequestModel';
import type { PublishDocumentRequestModel } from '../models/PublishDocumentRequestModel';
import type { PublishDocumentWithDescendantsRequestModel } from '../models/PublishDocumentWithDescendantsRequestModel';
import type { SortingRequestModel } from '../models/SortingRequestModel';
import type { UnpublishDocumentRequestModel } from '../models/UnpublishDocumentRequestModel';
import type { UpdateDocumentNotificationsRequestModel } from '../models/UpdateDocumentNotificationsRequestModel';
import type { UpdateDocumentRequestModel } from '../models/UpdateDocumentRequestModel';
import type { UpdateDomainsRequestModel } from '../models/UpdateDomainsRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DocumentResource {

    /**
     * @returns PagedDocumentCollectionResponseModel Success
     * @throws ApiError
     */
    public static getCollectionDocumentById({
        id,
        dataTypeId,
        orderBy = 'updateDate',
        orderCulture,
        orderDirection,
        filter,
        skip,
        take = 100,
    }: {
        id: string,
        dataTypeId?: string,
        orderBy?: string,
        orderCulture?: string,
        orderDirection?: DirectionModel,
        filter?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedDocumentCollectionResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/collection/document/{id}',
            path: {
                'id': id,
            },
            query: {
                'dataTypeId': dataTypeId,
                'orderBy': orderBy,
                'orderCulture': orderCulture,
                'orderDirection': orderDirection,
                'filter': filter,
                'skip': skip,
                'take': take,
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
            responseHeader: 'Umb-Generated-Resource',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDocumentById({
        id,
    }: {
        id: string,
    }): CancelablePromise<DocumentResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document/{id}',
            path: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteDocumentById({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/document/{id}',
            path: {
                'id': id,
            },
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putDocumentById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateDocumentRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postDocumentByIdCopy({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: CopyDocumentRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/document/{id}/copy',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Generated-Resource',
            errors: {
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDocumentByIdDomains({
        id,
    }: {
        id: string,
    }): CancelablePromise<DomainsResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document/{id}/domains',
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
    public static putDocumentByIdDomains({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateDomainsRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document/{id}/domains',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putDocumentByIdMove({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: MoveDocumentRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document/{id}/move',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putDocumentByIdMoveToRecycleBin({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document/{id}/move-to-recycle-bin',
            path: {
                'id': id,
            },
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDocumentByIdNotifications({
        id,
    }: {
        id: string,
    }): CancelablePromise<Array<DocumentNotificationResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document/{id}/notifications',
            path: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putDocumentByIdNotifications({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateDocumentNotificationsRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document/{id}/notifications',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                401: `The resource is protected and requires an authentication token`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postDocumentByIdPublicAccess({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: PublicAccessRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/document/{id}/public-access',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Umb-Generated-Resource',
            errors: {
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteDocumentByIdPublicAccess({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/document/{id}/public-access',
            path: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns void
     * @throws ApiError
     */
    public static getDocumentByIdPublicAccess({
        id,
    }: {
        id: string,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document/{id}/public-access',
            path: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putDocumentByIdPublicAccess({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: PublicAccessRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document/{id}/public-access',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putDocumentByIdPublish({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: (PublishDocumentRequestModel | PublishDocumentWithDescendantsRequestModel),
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document/{id}/publish',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putDocumentByIdPublishWithDescendants({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: PublishDocumentWithDescendantsRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document/{id}/publish-with-descendants',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putDocumentByIdUnpublish({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UnpublishDocumentRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document/{id}/unpublish',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putDocumentByIdValidate({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateDocumentRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document/{id}/validate',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDocumentConfiguration(): CancelablePromise<DocumentConfigurationResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/document/configuration',
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putDocumentSort({
        requestBody,
    }: {
        requestBody?: SortingRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/document/sort',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postDocumentValidate({
        requestBody,
    }: {
        requestBody?: CreateDocumentRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/document/validate',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getItemDocument({
        id,
    }: {
        id?: Array<string>,
    }): CancelablePromise<Array<DocumentItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/item/document',
            query: {
                'id': id,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteRecycleBinDocument(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/recycle-bin/document',
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteRecycleBinDocumentById({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/recycle-bin/document/{id}',
            path: {
                'id': id,
            },
            errors: {
                400: `Bad Request`,
                401: `The resource is protected and requires an authentication token`,
                403: `The authenticated user do not have access to this resource`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns PagedDocumentRecycleBinItemResponseModel Success
     * @throws ApiError
     */
    public static getRecycleBinDocumentChildren({
        parentId,
        skip,
        take = 100,
    }: {
        parentId?: string,
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedDocumentRecycleBinItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/recycle-bin/document/children',
            query: {
                'parentId': parentId,
                'skip': skip,
                'take': take,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns PagedDocumentRecycleBinItemResponseModel Success
     * @throws ApiError
     */
    public static getRecycleBinDocumentRoot({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedDocumentRecycleBinItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/recycle-bin/document/root',
            query: {
                'skip': skip,
                'take': take,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

    /**
     * @returns PagedDocumentTreeItemResponseModel Success
     * @throws ApiError
     */
    public static getTreeDocumentChildren({
        parentId,
        skip,
        take = 100,
        dataTypeId,
    }: {
        parentId?: string,
        skip?: number,
        take?: number,
        dataTypeId?: string,
    }): CancelablePromise<PagedDocumentTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document/children',
            query: {
                'parentId': parentId,
                'skip': skip,
                'take': take,
                'dataTypeId': dataTypeId,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
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
        dataTypeId,
    }: {
        skip?: number,
        take?: number,
        dataTypeId?: string,
    }): CancelablePromise<PagedDocumentTreeItemResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/document/root',
            query: {
                'skip': skip,
                'take': take,
                'dataTypeId': dataTypeId,
            },
            errors: {
                401: `The resource is protected and requires an authentication token`,
            },
        });
    }

}
