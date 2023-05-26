/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentValueModel } from './DocumentValueModel';
import type { DocumentVariantResponseModel } from './DocumentVariantResponseModel';

export type ContentResponseModelBaseDocumentValueModelDocumentVariantResponseModel = {
    values?: Array<DocumentValueModel>;
    variants?: Array<DocumentVariantResponseModel>;
    id?: string;
    contentTypeId?: string;
};

