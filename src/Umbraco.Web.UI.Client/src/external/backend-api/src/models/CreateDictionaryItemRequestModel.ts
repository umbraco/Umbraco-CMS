/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DictionaryItemModelBaseModel } from './DictionaryItemModelBaseModel';

export type CreateDictionaryItemRequestModel = (DictionaryItemModelBaseModel & {
parentId?: string | null;
});
