/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { TextFileViewModelBaseModel } from './TextFileViewModelBaseModel';

export type CreateTextFileViewModelBaseModel = (TextFileViewModelBaseModel & {
parentPath?: string | null;
});
