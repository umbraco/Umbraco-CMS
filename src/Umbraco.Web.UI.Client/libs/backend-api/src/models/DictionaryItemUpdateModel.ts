/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DictionaryItemTranslationModel } from './DictionaryItemTranslationModel';

export type DictionaryItemUpdateModel = {
    name?: string;
    translations?: Array<DictionaryItemTranslationModel>;
};

