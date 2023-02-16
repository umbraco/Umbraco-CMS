/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentUrlInfoModel } from './ContentUrlInfoModel';
import type { ContentViewModelBaseDocumentPropertyDocumentVariantModel } from './ContentViewModelBaseDocumentPropertyDocumentVariantModel';

export type DocumentModel = (ContentViewModelBaseDocumentPropertyDocumentVariantModel & {
    urls?: Array<ContentUrlInfoModel>;
    templateKey?: string | null;
});

