/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { SnippetItemResponseModel } from './SnippetItemResponseModel';

export type PagedSnippetItemResponseModel = {
    total: number;
    items: Array<SnippetItemResponseModel>;
};
