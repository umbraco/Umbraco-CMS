import { UmbWebhooksRepository } from '../repository/webhooks.repository.js';
import { UmbBaseController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UMB_WORKSPACE_CONTEXT, UmbWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';

// TODO: Revisit usage of workspace for this case...
export class UmbWebhooksWorkspaceContext extends UmbBaseController implements UmbWorkspaceContextInterface {
	public readonly workspaceAlias: string = 'Umb.Workspace.Webhooks';
	#repository: UmbWebhooksRepository;

	getEntityType() {
		return 'webhooks';
	}

	getEntityName() {
		return 'Webhooks';
	}

	getEntityId() {
		return undefined;
	}

	currentPage = 1;

	constructor(host: UmbControllerHostElement) {
		super(host);
		this.provideContext(UMB_WORKSPACE_CONTEXT, this);
		// TODO: Revisit usage of workspace for this case... currently no other workspace context provides them self with their own token.
		this.provideContext(UMB_APP_WEBHOOKS_CONTEXT_TOKEN, this);
		this.#repository = new UmbWebhooksRepository(host);
	}

	onChangeState = () => {
		
	};
}

export const UMB_APP_WEBHOOKS_CONTEXT_TOKEN = new UmbContextToken<UmbWebhooksWorkspaceContext>(
	UmbWebhooksWorkspaceContext.name,
);
