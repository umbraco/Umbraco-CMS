/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentPropertyModel } from './DocumentPropertyModel';
import type { DocumentVariantModel } from './DocumentVariantModel';

export type ContentViewModelBaseDocumentPropertyDocumentVariantModel = {
    key?: string;
    contentTypeKey?: string;
    properties?: Array<DocumentPropertyModel>;
    variants?: Array<DocumentVariantModel>;
};

