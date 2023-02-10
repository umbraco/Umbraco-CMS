/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { LanguageModelBaseModel } from './LanguageModelBaseModel';

export type LanguageCreateModel = (LanguageModelBaseModel & {
    isoCode?: string;
});

