/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentAppBadge } from './ContentAppBadge';

export type ContentApp = {
    name?: string | null;
    alias?: string | null;
    weight?: number;
    icon?: string | null;
    view?: string | null;
    viewModel?: any;
    active?: boolean;
    badge?: ContentAppBadge;
};

