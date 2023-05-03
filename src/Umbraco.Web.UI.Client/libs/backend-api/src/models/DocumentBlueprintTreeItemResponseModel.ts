/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { EntityTreeItemResponseModel } from './EntityTreeItemResponseModel';

export type DocumentBlueprintTreeItemResponseModel = (EntityTreeItemResponseModel & {
$type: string;
documentTypeId?: string;
documentTypeAlias?: string;
documentTypeName?: string | null;
});
