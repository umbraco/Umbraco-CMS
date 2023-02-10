/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTreeItemModel } from './ContentTreeItemModel';

export type DocumentTreeItemModel = (ContentTreeItemModel & {
    isProtected?: boolean;
    isPublished?: boolean;
    isEdited?: boolean;
});

