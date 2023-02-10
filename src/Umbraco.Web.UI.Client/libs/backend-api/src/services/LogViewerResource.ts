/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DirectionModel } from '../models/DirectionModel';
import type { LogLevelModel } from '../models/LogLevelModel';
import type { PagedLoggerModel } from '../models/PagedLoggerModel';
import type { PagedLogMessageModel } from '../models/PagedLogMessageModel';
import type { PagedLogTemplateModel } from '../models/PagedLogTemplateModel';
import type { PagedSavedLogSearchModel } from '../models/PagedSavedLogSearchModel';
import type { SavedLogSearchModel } from '../models/SavedLogSearchModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class LogViewerResource {

    /**
     * @returns PagedLoggerModel Success
     * @throws ApiError
     */
    public static getLogViewerLevel({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedLoggerModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/log-viewer/level',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getLogViewerLevelCount({
        startDate,
        endDate,
    }: {
        startDate?: string,
        endDate?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/log-viewer/level-count',
            query: {
                'startDate': startDate,
                'endDate': endDate,
            },
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns PagedLogMessageModel Success
     * @throws ApiError
     */
    public static getLogViewerLog({
        skip,
        take = 100,
        orderDirection,
        filterExpression,
        logLevel,
        startDate,
        endDate,
    }: {
        skip?: number,
        take?: number,
        orderDirection?: DirectionModel,
        filterExpression?: string,
        logLevel?: Array<LogLevelModel>,
        startDate?: string,
        endDate?: string,
    }): CancelablePromise<PagedLogMessageModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/log-viewer/log',
            query: {
                'skip': skip,
                'take': take,
                'orderDirection': orderDirection,
                'filterExpression': filterExpression,
                'logLevel': logLevel,
                'startDate': startDate,
                'endDate': endDate,
            },
        });
    }

    /**
     * @returns PagedLogTemplateModel Success
     * @throws ApiError
     */
    public static getLogViewerMessageTemplate({
        skip,
        take = 100,
        startDate,
        endDate,
    }: {
        skip?: number,
        take?: number,
        startDate?: string,
        endDate?: string,
    }): CancelablePromise<PagedLogTemplateModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/log-viewer/message-template',
            query: {
                'skip': skip,
                'take': take,
                'startDate': startDate,
                'endDate': endDate,
            },
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns PagedSavedLogSearchModel Success
     * @throws ApiError
     */
    public static getLogViewerSavedSearch({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedSavedLogSearchModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/log-viewer/saved-search',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns any Created
     * @throws ApiError
     */
    public static postLogViewerSavedSearch({
        requestBody,
    }: {
        requestBody?: SavedLogSearchModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/log-viewer/saved-search',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getLogViewerSavedSearchByName({
        name,
    }: {
        name: string,
    }): CancelablePromise<SavedLogSearchModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/log-viewer/saved-search/{name}',
            path: {
                'name': name,
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
    public static deleteLogViewerSavedSearchByName({
        name,
    }: {
        name: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/log-viewer/saved-search/{name}',
            path: {
                'name': name,
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
    public static getLogViewerValidateLogsSize({
        startDate,
        endDate,
    }: {
        startDate?: string,
        endDate?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/log-viewer/validate-logs-size',
            query: {
                'startDate': startDate,
                'endDate': endDate,
            },
            errors: {
                400: `Bad Request`,
            },
        });
    }

}
