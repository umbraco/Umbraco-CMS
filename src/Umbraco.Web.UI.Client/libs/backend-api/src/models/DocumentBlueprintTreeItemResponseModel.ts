/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { EntityTreeItemResponseModel } from './EntityTreeItemResponseModel';

export type DocumentBlueprintTreeItemResponseModel = (EntityTreeItemResponseModel & {
$type: string;
documentTypeKey?: string;
documentTypeAlias?: string;
documentTypeName?: string | null;
});
