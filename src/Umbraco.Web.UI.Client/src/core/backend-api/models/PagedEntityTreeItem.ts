/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { EntityTreeItem } from './EntityTreeItem';

export type PagedEntityTreeItem = {
    total?: number;
    items?: Array<EntityTreeItem> | null;
};

