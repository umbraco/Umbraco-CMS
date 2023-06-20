/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentResponseModelBaseDocumentValueModelDocumentVariantResponseModel } from './ContentResponseModelBaseDocumentValueModelDocumentVariantResponseModel';
import type { ContentUrlInfoModel } from './ContentUrlInfoModel';

export type DocumentResponseModel = (ContentResponseModelBaseDocumentValueModelDocumentVariantResponseModel & {
urls?: Array<ContentUrlInfoModel>;
templateId?: string | null;
});
