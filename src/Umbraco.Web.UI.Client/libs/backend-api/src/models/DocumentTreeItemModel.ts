/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTreeItemModel } from './ContentTreeItemModel';

export type DocumentTreeItemModel = (ContentTreeItemModel & {
    $type: string;
    isProtected?: boolean;
    isPublished?: boolean;
    isEdited?: boolean;
});

