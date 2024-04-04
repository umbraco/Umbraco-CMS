/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { AuditLogEntityModel } from './AuditLogEntityModel';
import type { AuditTypeModel } from './AuditTypeModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type AuditLogResponseModel = {
    user: ReferenceByIdModel;
    entity: AuditLogEntityModel;
    timestamp: string;
    logType: AuditTypeModel;
    comment?: string | null;
    parameters?: string | null;
};

