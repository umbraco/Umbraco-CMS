import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import type { UmbWebhookWorkspaceContext } from './webhook-workspace.context.js';

export const UMB_WEBHOOK_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSubmittableWorkspaceContext,
	UmbWebhookWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbWebhookWorkspaceContext => context.getEntityType?.() === 'webhook',
);
