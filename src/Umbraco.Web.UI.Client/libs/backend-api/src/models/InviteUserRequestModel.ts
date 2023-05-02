/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CreateUserRequestModel } from './CreateUserRequestModel';

export type InviteUserRequestModel = (CreateUserRequestModel & {
message?: string | null;
});
