/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { VerifyInviteUserRequestModel } from './VerifyInviteUserRequestModel';

export type CreateInitialPasswordUserRequestModel = (VerifyInviteUserRequestModel & {
    password?: string;
});

