/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PackageCreateModel } from '../models/PackageCreateModel';
import type { PackageDefinitionModel } from '../models/PackageDefinitionModel';
import type { PackageManifestModel } from '../models/PackageManifestModel';
import type { PackageUpdateModel } from '../models/PackageUpdateModel';
import type { PagedPackageDefinitionModel } from '../models/PagedPackageDefinitionModel';
import type { PagedPackageMigrationStatusModel } from '../models/PagedPackageMigrationStatusModel';

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
     * @returns PagedPackageDefinitionModel Success
     * @throws ApiError
     */
    public static getPackageCreated({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedPackageDefinitionModel> {
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
        requestBody?: PackageCreateModel,
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
    public static getPackageCreatedByKey({
        key,
    }: {
        key: string,
    }): CancelablePromise<PackageDefinitionModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/package/created/{key}',
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
    public static deletePackageCreatedByKey({
        key,
    }: {
        key: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/package/created/{key}',
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
    public static putPackageCreatedByKey({
        key,
        requestBody,
    }: {
        key: string,
        requestBody?: PackageUpdateModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/package/created/{key}',
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
     * @returns binary Success
     * @throws ApiError
     */
    public static getPackageCreatedByKeyDownload({
        key,
    }: {
        key: string,
    }): CancelablePromise<Blob> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/package/created/{key}/download',
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
    public static getPackageManifest(): CancelablePromise<Array<PackageManifestModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/package/manifest',
        });
    }

    /**
     * @returns PagedPackageMigrationStatusModel Success
     * @throws ApiError
     */
    public static getPackageMigrationStatus({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedPackageMigrationStatusModel> {
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
