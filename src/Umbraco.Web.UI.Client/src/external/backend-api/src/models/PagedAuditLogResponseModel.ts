/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { AuditLogResponseModel } from './AuditLogResponseModel';

export type PagedAuditLogResponseModel = {
    total: number;
    items: Array<AuditLogResponseModel>;
};
