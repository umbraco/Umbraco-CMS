/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DictionaryItemsImportModel } from './DictionaryItemsImportModel';

export type DictionaryUploadModel = {
    dictionaryItems?: Array<DictionaryItemsImportModel>;
    fileName?: string | null;
};

