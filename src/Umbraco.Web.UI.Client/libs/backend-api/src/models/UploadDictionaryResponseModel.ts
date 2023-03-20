/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ImportDictionaryItemsPresentationModel } from './ImportDictionaryItemsPresentationModel';

export type UploadDictionaryResponseModel = {
    dictionaryItems?: Array<ImportDictionaryItemsPresentationModel>;
    fileName?: string | null;
};
