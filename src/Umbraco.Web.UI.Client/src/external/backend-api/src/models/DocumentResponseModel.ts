/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentForDocumentResponseModel } from './ContentForDocumentResponseModel';
import type { ContentUrlInfoModel } from './ContentUrlInfoModel';

export type DocumentResponseModel = (ContentForDocumentResponseModel & {
    urls?: Array<ContentUrlInfoModel>;
    templateId?: string | null;
    isTrashed?: boolean;
});

