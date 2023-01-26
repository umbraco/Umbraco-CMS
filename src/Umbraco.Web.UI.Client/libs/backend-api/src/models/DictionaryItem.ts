/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DictionaryItemTranslationModel } from './DictionaryItemTranslationModel';

export type DictionaryItem = {
    name?: string;
    translations?: Array<DictionaryItemTranslationModel>;
    key?: string;
};

