/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreatePackageRequestModel } from '../models/CreatePackageRequestModel';
import type { PackageDefinitionResponseModel } from '../models/PackageDefinitionResponseModel';
import type { PackageManifestResponseModel } from '../models/PackageManifestResponseModel';
import type { PagedPackageDefinitionResponseModel } from '../models/PagedPackageDefinitionResponseModel';
import type { PagedPackageMigrationStatusResponseModel } from '../models/PagedPackageMigrationStatusResponseModel';
import type { UpdatePackageRequestModel } from '../models/UpdatePackageRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class PackageResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postPackageByNameRunMigration({
        name,
    }: {
        name: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/package/{name}/run-migration',
            path: {
                'name': name,
            },
            errors: {
                404: `Not Found`,
                409: `Conflict`,
            },
        });
    }

    /**
     * @returns PagedPackageDefinitionResponseModel Success
     * @throws ApiError
     */
    public static getPackageCreated({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedPackageDefinitionResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/package/created',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postPackageCreated({
        requestBody,
    }: {
        requestBody?: CreatePackageRequestModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/package/created',
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
    public static getPackageCreatedById({
        id,
    }: {
        id: string,
    }): CancelablePromise<PackageDefinitionResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/package/created/{id}',
            path: {
                'id': id,
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
    public static deletePackageCreatedById({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/package/created/{id}',
            path: {
                'id': id,
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
    public static putPackageCreatedById({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdatePackageRequestModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/package/created/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns binary Success
     * @throws ApiError
     */
    public static getPackageCreatedByIdDownload({
        id,
    }: {
        id: string,
    }): CancelablePromise<Blob> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/package/created/{id}/download',
            path: {
                'id': id,
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
    public static getPackageManifest(): CancelablePromise<Array<PackageManifestResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/package/manifest',
        });
    }

    /**
     * @returns PagedPackageMigrationStatusResponseModel Success
     * @throws ApiError
     */
    public static getPackageMigrationStatus({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedPackageMigrationStatusResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/package/migration-status',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
