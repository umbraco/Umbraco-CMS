/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { EntityTreeItemResponseModel } from './EntityTreeItemResponseModel';

export type ContentTreeItemResponseModel = (EntityTreeItemResponseModel & {
    $type: string;
    noAccess?: boolean;
    isTrashed?: boolean;
});

