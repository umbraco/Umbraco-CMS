/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentValueModel } from './DocumentValueModel';
import type { DocumentVariantModel } from './DocumentVariantModel';

export type ContentViewModelBaseDocumentValueDocumentVariantModel = {
    key?: string;
    contentTypeKey?: string;
    values?: Array<DocumentValueModel>;
    variants?: Array<DocumentVariantModel>;
};

