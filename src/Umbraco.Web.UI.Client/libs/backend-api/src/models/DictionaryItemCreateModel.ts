/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DictionaryItemTranslationModel } from './DictionaryItemTranslationModel';

export type DictionaryItemCreateModel = {
    name?: string;
    translations?: Array<DictionaryItemTranslationModel>;
    parentKey?: string | null;
};

