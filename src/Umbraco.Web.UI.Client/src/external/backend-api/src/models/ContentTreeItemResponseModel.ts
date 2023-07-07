/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { EntityTreeItemResponseModel } from './EntityTreeItemResponseModel';

export type ContentTreeItemResponseModel = (EntityTreeItemResponseModel & {
    noAccess?: boolean;
    isTrashed?: boolean;
    id?: string;
});

