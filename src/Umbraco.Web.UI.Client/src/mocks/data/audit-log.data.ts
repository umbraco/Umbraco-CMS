import { data as userData } from './user/user.data.js';
import type { AuditLogResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { AuditTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

const userId = userData[0].id;

export const logs: Array<AuditLogResponseModel> = [
	{
		user: { id: userId },
		timestamp: '2021-09-14T09:32:49.0000000Z',
		logType: AuditTypeModel.SAVE,
		comment: null,
		parameters: null,
	},
	{
		user: { id: userId },
		timestamp: '2022-09-14T11:30:49.0000000Z',
		logType: AuditTypeModel.SAVE,
		comment: null,
		parameters: null,
	},
	{
		user: { id: userId },
		timestamp: '2022-09-15T09:35:49.0000000Z',
		logType: AuditTypeModel.SAVE,
		comment: null,
		parameters: null,
	},
	{
		user: { id: userId },
		timestamp: '2023-01-09T12:00:00.0000000Z',
		logType: AuditTypeModel.PUBLISH,
		comment: null,
		parameters: null,
	},
];
