/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTreeItemResponseModel } from './ContentTreeItemResponseModel';

export type DocumentTreeItemResponseModel = (ContentTreeItemResponseModel & {
    $type: string;
    isProtected?: boolean;
    isPublished?: boolean;
    isEdited?: boolean;
});

