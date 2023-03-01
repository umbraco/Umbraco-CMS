/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { UserGroupBaseModel } from './UserGroupBaseModel';

export type UserGroupModel = (UserGroupBaseModel & {
    $type: string;
    key?: string;
});

