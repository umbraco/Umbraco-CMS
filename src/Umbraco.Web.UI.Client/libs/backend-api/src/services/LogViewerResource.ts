/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { Direction } from '../models/Direction';
import type { LogLevel } from '../models/LogLevel';
import type { PagedLogger } from '../models/PagedLogger';
import type { PagedLogMessage } from '../models/PagedLogMessage';
import type { PagedLogTemplate } from '../models/PagedLogTemplate';
import type { PagedSavedLogSearch } from '../models/PagedSavedLogSearch';
import type { SavedLogSearch } from '../models/SavedLogSearch';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class LogViewerResource {

    /**
     * @returns PagedLogger Success
     * @throws ApiError
     */
    public static getLogViewerLevel({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedLogger> {
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
     * @returns PagedLogMessage Success
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
        orderDirection?: Direction,
        filterExpression?: string,
        logLevel?: Array<LogLevel>,
        startDate?: string,
        endDate?: string,
    }): CancelablePromise<PagedLogMessage> {
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
     * @returns PagedLogTemplate Success
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
    }): CancelablePromise<PagedLogTemplate> {
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
     * @returns PagedSavedLogSearch Success
     * @throws ApiError
     */
    public static getLogViewerSavedSearch({
        skip,
        take = 100,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<PagedSavedLogSearch> {
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
        requestBody?: SavedLogSearch,
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
     * @returns SavedLogSearch Success
     * @throws ApiError
     */
    public static getLogViewerSavedSearchByName({
        name,
    }: {
        name: string,
    }): CancelablePromise<SavedLogSearch> {
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
