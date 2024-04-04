/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentTypeReferenceResponseModel } from './DocumentTypeReferenceResponseModel';
import type { DocumentUrlInfoModel } from './DocumentUrlInfoModel';
import type { DocumentValueModel } from './DocumentValueModel';
import type { DocumentVariantResponseModel } from './DocumentVariantResponseModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type DocumentResponseModel = {
    values: Array<DocumentValueModel>;
    variants: Array<DocumentVariantResponseModel>;
    id: string;
    documentType: DocumentTypeReferenceResponseModel;
    urls: Array<DocumentUrlInfoModel>;
    template: ReferenceByIdModel;
    isTrashed: boolean;
};

