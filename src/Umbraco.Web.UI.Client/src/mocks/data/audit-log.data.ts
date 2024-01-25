import { data as userData } from './user/user.data.js';
import { data as documentData } from './document.data.js';
import type {
	AuditLogResponseModel,
	AuditLogWithUsernameResponseModel} from '@umbraco-cms/backoffice/backend-api';
import {
	AuditTypeModel,
} from '@umbraco-cms/backoffice/backend-api';

const userId = userData[0].id;
const userName = userData[0].name;
const userAvatars = userData[0].avatarUrls;

const documentId = documentData[0].id;

export const logs: Array<AuditLogResponseModel> = [
	{
		userId: userId,
		entityId: documentId,
		entityType: 'Document',
		timestamp: '2021-09-14T09:32:49.0000000Z',
		logType: AuditTypeModel.SAVE,
		comment: undefined,
		parameters: undefined,
	},
	{
		userId: userId,
		entityId: documentId,
		entityType: 'Document',
		timestamp: '2022-09-14T11:30:49.0000000Z',
		logType: AuditTypeModel.SAVE,
		comment: undefined,
		parameters: undefined,
	},
	{
		userId: userId,
		entityId: documentId,
		entityType: 'Document',
		timestamp: '2022-09-15T09:35:49.0000000Z',
		logType: AuditTypeModel.SAVE,
		comment: undefined,
		parameters: undefined,
	},
	{
		userId: userId,
		entityId: documentId,
		entityType: 'Document',
		timestamp: '2023-01-09T12:00:00.0000000Z',
		logType: AuditTypeModel.PUBLISH,
		comment: undefined,
		parameters: undefined,
	},
];

export const logsWithUser: Array<AuditLogWithUsernameResponseModel> = logs.map((log) => ({
	...log,
	userName: userName,
	userAvatars: userAvatars,
}));
