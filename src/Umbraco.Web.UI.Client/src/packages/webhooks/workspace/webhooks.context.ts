import { UmbWebhookRepository } from '../repository/webhooks.repository.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

// TODO: Revisit usage of workspace for this case...
export class UmbWebhooksWorkspaceContext extends UmbBaseController implements UmbWorkspaceContextInterface {
	public readonly workspaceAlias = 'Umb.Workspace.Webhooks';
	#repository;

	getEntityType() {
		return 'webhooks';
	}

	getEntityName() {
		return 'Webhooks';
	}

	getUnique() {
		return undefined;
	}

	currentPage = 1;

	constructor(host: UmbControllerHostElement) {
		super(host);
		this.provideContext(UMB_WORKSPACE_CONTEXT, this);
		// TODO: Revisit usage of workspace for this case... currently no other workspace context provides them self with their own token.
		this.provideContext(UMB_APP_WEBHOOKS_CONTEXT, this);
		this.#repository = new UmbWebhookRepository(host);
	}
}

export const UMB_APP_WEBHOOKS_CONTEXT = new UmbContextToken<UmbWebhooksWorkspaceContext>(
	UmbWebhooksWorkspaceContext.name,
);
