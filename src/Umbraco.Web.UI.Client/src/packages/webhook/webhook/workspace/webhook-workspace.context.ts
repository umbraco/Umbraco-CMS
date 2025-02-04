import type { UmbWebhookDetailRepository } from '../repository/index.js';
import { UMB_WEBHOOK_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_WEBHOOK_ENTITY_TYPE, UMB_WEBHOOK_ROOT_ENTITY_TYPE, UMB_WEBHOOK_WORKSPACE_ALIAS } from '../../entity.js';
import type { UmbWebhookDetailModel } from '../types.js';
import type { UmbWebhookEventModel } from '../../webhook-event/types.js';
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
	// Observable states
	readonly headers = this._data.createObservablePartOfCurrent((data) => data?.headers);
	readonly enabled = this._data.createObservablePartOfCurrent((data) => data?.enabled);
	readonly url = this._data.createObservablePartOfCurrent((data) => data?.url);
	readonly name = this._data.createObservablePartOfCurrent((data) => data?.name);
	readonly description = this._data.createObservablePartOfCurrent((data) => data?.description);
	readonly events = this._data.createObservablePartOfCurrent((data) => data?.events);
	readonly contentTypes = this._data.createObservablePartOfCurrent((data) => data?.contentTypes);

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

	/**
	 * Sets the events
	 * @param {Array<UmbWebhookEventModel>} events - The events
	 * @memberof UmbWebhookWorkspaceContext
	 */
	setEvents(events: Array<UmbWebhookEventModel>) {
		this._data.updateCurrent({ events });
	}

	/**
	 * Gets the events
	 * @returns {Array<UmbWebhookEventModel>}
	 * @memberof UmbWebhookWorkspaceContext
	 */
	getEvents(): Array<UmbWebhookEventModel> | undefined {
		return this._data.getCurrent()?.events;
	}

	/**
	 * Sets the headers
	 * @param {{ [key: string]: string }} headers - The headers
	 * @memberof UmbWebhookWorkspaceContext
	 */
	setHeaders(headers: { [key: string]: string }) {
		this._data.updateCurrent({ headers });
	}

	/**
	 * Gets the headers
	 * @returns {UmbWebhookDetailModel['headers']} - The headers
	 * @memberof UmbWebhookWorkspaceContext
	 */
	getHeaders(): UmbWebhookDetailModel['headers'] | undefined {
		return this._data.getCurrent()?.headers;
	}

	/**
	 * Sets the content types
	 * @param {string[]} types - The content types
	 * @memberof UmbWebhookWorkspaceContext
	 */
	setTypes(types: string[]) {
		this._data.updateCurrent({ contentTypes: types });
	}

	/**
	 * Gets the content types
	 * @returns {string[]} - The content types
	 * @memberof UmbWebhookWorkspaceContext
	 */
	getTypes(): string[] | undefined {
		return this._data.getCurrent()?.contentTypes;
	}

	/**
	 * Sets the URL
	 * @param {string} url - The URL
	 * @memberof UmbWebhookWorkspaceContext
	 */
	setUrl(url: string) {
		this._data.updateCurrent({ url });
	}

	/**
	 * Sets the name
	 * @param {string} name - The name
	 * @memberof UmbWebhookWorkspaceContext
	 */
	setName(name: string) {
		this._data.updateCurrent({ name });
	}

	/**
	 * Sets the description
	 * @param {string} description - The description
	 * @memberof UmbWebhookWorkspaceContext
	 */
	setDescription(description: string) {
		this._data.updateCurrent({ description });
	}

	/**
	 * Gets the URL
	 * @returns {string} - The URL
	 * @memberof UmbWebhookWorkspaceContext
	 */
	getUrl(): string | undefined {
		return this._data.getCurrent()?.url;
	}

	/**
	 * Sets the enabled state
	 * @param {boolean} enabled - The enabled state
	 * @memberof UmbWebhookWorkspaceContext
	 */
	setEnabled(enabled: boolean) {
		this._data.updateCurrent({ enabled });
	}

	/**
	 * Gets the enabled state
	 * @returns {boolean} - The enabled state
	 * @memberof UmbWebhookWorkspaceContext
	 */
	getEnabled(): boolean | undefined {
		return this._data.getCurrent()?.enabled;
	}
}

export { UmbWebhookWorkspaceContext as api };
