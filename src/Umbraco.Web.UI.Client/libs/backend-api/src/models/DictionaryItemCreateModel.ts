/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DictionaryItemModelBaseModel } from './DictionaryItemModelBaseModel';

export type DictionaryItemCreateModel = (DictionaryItemModelBaseModel & {
    parentKey?: string | null;
});

