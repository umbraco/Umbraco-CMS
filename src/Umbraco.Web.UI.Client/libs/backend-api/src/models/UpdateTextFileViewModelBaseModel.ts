/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { TextFileViewModelBaseModel } from './TextFileViewModelBaseModel';

export type UpdateTextFileViewModelBaseModel = (TextFileViewModelBaseModel & {
    existingPath?: string;
});

