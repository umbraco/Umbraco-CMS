/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DictionaryItemTranslationModel } from './DictionaryItemTranslationModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type CreateDictionaryItemRequestModel = {
    name: string;
    translations: Array<DictionaryItemTranslationModel>;
    id?: string | null;
    parent?: ReferenceByIdModel | null;
};

