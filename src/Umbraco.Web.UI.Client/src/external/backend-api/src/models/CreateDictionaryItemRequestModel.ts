/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DictionaryItemModelBaseModel } from './DictionaryItemModelBaseModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type CreateDictionaryItemRequestModel = (DictionaryItemModelBaseModel & {
    id?: string | null;
    parent?: ReferenceByIdModel | null;
});

