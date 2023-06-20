/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreatePathFolderRequestModel } from '../models/CreatePathFolderRequestModel';
import type { CreateStylesheetRequestModel } from '../models/CreateStylesheetRequestModel';
import type { ExtractRichTextStylesheetRulesRequestModel } from '../models/ExtractRichTextStylesheetRulesRequestModel';
import type { ExtractRichTextStylesheetRulesResponseModel } from '../models/ExtractRichTextStylesheetRulesResponseModel';
import type { InterpolateRichTextStylesheetRequestModel } from '../models/InterpolateRichTextStylesheetRequestModel';
import type { InterpolateRichTextStylesheetResponseModel } from '../models/InterpolateRichTextStylesheetResponseModel';
import type { PagedFileSystemTreeItemPresentationModel } from '../models/PagedFileSystemTreeItemPresentationModel';
import type { PagedStylesheetOverviewResponseModel } from '../models/PagedStylesheetOverviewResponseModel';
import type { RichTextStylesheetRulesResponseModel } from '../models/RichTextStylesheetRulesResponseModel';
import type { ScriptItemResponseModel } from '../models/ScriptItemResponseModel';
import type { StylesheetResponseModel } from '../models/StylesheetResponseModel';
import type { UpdateStylesheetRequestModel } from '../models/UpdateStylesheetRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class StylesheetResource {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getStylesheet({
path,
}: {
path?: string,
}): CancelablePromise<StylesheetResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/stylesheet',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns string Created
     * @throws ApiError
     */
    public static postStylesheet({
requestBody,
}: {
requestBody?: CreateStylesheetRequestModel,
}): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/stylesheet',
            body: requestBody,
            mediaType: 'application/json',
            responseHeader: 'Location',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteStylesheet({
path,
}: {
path?: string,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/stylesheet',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static putStylesheet({
requestBody,
}: {
requestBody?: UpdateStylesheetRequestModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/umbraco/management/api/v1/stylesheet',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns PagedStylesheetOverviewResponseModel Success
     * @throws ApiError
     */
    public static getStylesheetAll({
skip,
take = 100,
}: {
skip?: number,
take?: number,
}): CancelablePromise<PagedStylesheetOverviewResponseModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/stylesheet/all',
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
    public static getStylesheetFolder({
path,
}: {
path?: string,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/stylesheet/folder',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postStylesheetFolder({
requestBody,
}: {
requestBody?: CreatePathFolderRequestModel,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/stylesheet/folder',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static deleteStylesheetFolder({
path,
}: {
path?: string,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/umbraco/management/api/v1/stylesheet/folder',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getStylesheetItem({
path,
}: {
path?: Array<string>,
}): CancelablePromise<Array<ScriptItemResponseModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/stylesheet/item',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postStylesheetRichTextExtractRules({
requestBody,
}: {
requestBody?: ExtractRichTextStylesheetRulesRequestModel,
}): CancelablePromise<ExtractRichTextStylesheetRulesResponseModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/stylesheet/rich-text/extract-rules',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static postStylesheetRichTextInterpolateRules({
requestBody,
}: {
requestBody?: InterpolateRichTextStylesheetRequestModel,
}): CancelablePromise<InterpolateRichTextStylesheetResponseModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/management/api/v1/stylesheet/rich-text/interpolate-rules',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getStylesheetRichTextRules({
path,
}: {
path?: string,
}): CancelablePromise<(RichTextStylesheetRulesResponseModel | ExtractRichTextStylesheetRulesResponseModel)> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/stylesheet/rich-text/rules',
            query: {
                'path': path,
            },
        });
    }

    /**
     * @returns PagedFileSystemTreeItemPresentationModel Success
     * @throws ApiError
     */
    public static getTreeStylesheetChildren({
path,
skip,
take = 100,
}: {
path?: string,
skip?: number,
take?: number,
}): CancelablePromise<PagedFileSystemTreeItemPresentationModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/stylesheet/children',
            query: {
                'path': path,
                'skip': skip,
                'take': take,
            },
        });
    }

    /**
     * @returns PagedFileSystemTreeItemPresentationModel Success
     * @throws ApiError
     */
    public static getTreeStylesheetRoot({
skip,
take = 100,
}: {
skip?: number,
take?: number,
}): CancelablePromise<PagedFileSystemTreeItemPresentationModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/management/api/v1/tree/stylesheet/root',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }

}
