import { UMB_WEBHOOK_ENTITY_TYPE, UMB_WEBHOOK_WORKSPACE } from '../../entity.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

export class UmbWebhookWorkspaceContext extends UmbControllerBase implements UmbWorkspaceContext {
	public readonly workspaceAlias = UMB_WEBHOOK_WORKSPACE;

	getEntityType() {
		return UMB_WEBHOOK_ENTITY_TYPE;
	}

	constructor(host: UmbControllerHost) {
		super(host);
		this.provideContext(UMB_WORKSPACE_CONTEXT, this);
		// TODO: Revisit usage of workspace for this case... currently no other workspace context provides them self with their own token.
		this.provideContext(UMB_APP_WEBHOOK_CONTEXT, this);
	}
}

export { UmbWebhookWorkspaceContext as api };

export const UMB_APP_WEBHOOK_CONTEXT = new UmbContextToken<UmbWebhookWorkspaceContext>(UmbWebhookWorkspaceContext.name);
