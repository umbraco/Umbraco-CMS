/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentUrlInfoModel } from './ContentUrlInfoModel';
import type { ContentViewModelBaseDocumentValueDocumentVariantModel } from './ContentViewModelBaseDocumentValueDocumentVariantModel';

export type DocumentModel = (ContentViewModelBaseDocumentValueDocumentVariantModel & {
    urls?: Array<ContentUrlInfoModel>;
    templateKey?: string | null;
});

