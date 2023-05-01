/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { UserResponseModel } from './UserResponseModel';

export type PagedUserResponseModel = {
    total: number;
    items: Array<UserResponseModel>;
};
