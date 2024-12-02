import type { UmbWebhookDetailModel, UmbWebhookEventModel } from '../../types.js';
import type { UmbWebhookDetailRepository } from '../../repository/index.js';
import { UMB_WEBHOOK_DETAIL_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UMB_WEBHOOK_ENTITY_TYPE, UMB_WEBHOOK_ROOT_ENTITY_TYPE, UMB_WEBHOOK_WORKSPACE_ALIAS } from '../../entity.js';
import { UmbWebhookWorkspaceEditorElement } from './webhook-workspace-editor.element.js';
import {
	type UmbSubmittableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
	type UmbRoutableWorkspaceContext,
	UmbEntityDetailWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbWebhookWorkspaceContext
	extends UmbEntityDetailWorkspaceContextBase<UmbWebhookDetailModel, UmbWebhookDetailRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_WEBHOOK_WORKSPACE_ALIAS,
			entityType: UMB_WEBHOOK_ENTITY_TYPE,
			detailRepositoryAlias: UMB_WEBHOOK_DETAIL_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'create',
				component: UmbWebhookWorkspaceEditorElement,
				setup: async () => {
					await this.createScaffold({ parent: { entityType: UMB_WEBHOOK_ROOT_ENTITY_TYPE, unique: null } });

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: UmbWebhookWorkspaceEditorElement,
				setup: (_component, info) => {
					this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
					this.load(info.match.params.unique);
				},
			},
		]);
	}

	setEvents(events: Array<UmbWebhookEventModel>) {
		this._data.updateCurrent({ events });
	}

	setHeaders(headers: { [key: string]: string }) {
		this._data.updateCurrent({ headers });
	}

	setTypes(types: string[]) {
		this._data.updateCurrent({ contentTypes: types });
	}

	setUrl(url: string) {
		this._data.updateCurrent({ url });
	}

	setEnabled(enabled: boolean) {
		this._data.updateCurrent({ enabled });
	}
}

export { UmbWebhookWorkspaceContext as api };
