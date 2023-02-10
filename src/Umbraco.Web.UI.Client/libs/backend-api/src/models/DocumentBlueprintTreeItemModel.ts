/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { EntityTreeItemModel } from './EntityTreeItemModel';

export type DocumentBlueprintTreeItemModel = (EntityTreeItemModel & {
    documentTypeKey?: string;
    documentTypeAlias?: string;
    documentTypeName?: string | null;
});

