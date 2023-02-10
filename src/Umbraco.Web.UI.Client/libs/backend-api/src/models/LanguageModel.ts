/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { LanguageModelBaseModel } from './LanguageModelBaseModel';

export type LanguageModel = (LanguageModelBaseModel & {
    isoCode?: string;
});

