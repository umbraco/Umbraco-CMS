/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { EntityTreeItemResponseModel } from './EntityTreeItemResponseModel';

export type DocumentBlueprintTreeItemResponseModel = (EntityTreeItemResponseModel & {
    documentTypeId?: string;
    documentTypeAlias?: string;
    documentTypeName?: string | null;
});

