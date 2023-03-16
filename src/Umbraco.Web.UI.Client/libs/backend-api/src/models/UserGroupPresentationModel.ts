/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { UserGroupBaseModel } from './UserGroupBaseModel';

export type UserGroupPresentationModel = (UserGroupBaseModel & {
$type: string;
key?: string;
});
