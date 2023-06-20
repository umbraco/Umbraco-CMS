/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { AuditLogBaseModel } from './AuditLogBaseModel';

export type AuditLogWithUsernameResponseModel = (AuditLogBaseModel & {
userName?: string | null;
userAvatars?: Array<string> | null;
});
