import type { UmbMockAuditLogModel } from '../data/mock-data-set.types.js';
import { UmbMockDBBase } from './utils/mock-db-base.js';

class UmbAuditLogMockDb extends UmbMockDBBase<UmbMockAuditLogModel> {
	constructor(data: Array<UmbMockAuditLogModel>) {
		super('auditLogs', data);
	}
}

export const umbAuditLogMockDb = new UmbAuditLogMockDb([]);
