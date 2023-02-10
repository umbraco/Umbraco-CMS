/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { EntityTreeItemModel } from './EntityTreeItemModel';

export type ContentTreeItemModel = (EntityTreeItemModel & {
    noAccess?: boolean;
    isTrashed?: boolean;
});

