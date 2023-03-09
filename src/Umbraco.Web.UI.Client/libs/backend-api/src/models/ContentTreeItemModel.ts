/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { EntityTreeItemModel } from './EntityTreeItemModel';

export type ContentTreeItemModel = (EntityTreeItemModel & {
    $type: string;
    noAccess?: boolean;
    isTrashed?: boolean;
});

