import type { UmbWebhookDetailModel } from '../../types.js';
import { UmbWebhookDetailRepository } from '../../repository/index.js';
import { UmbWebhookWorkspaceEditorElement } from './webhook-workspace-editor.element.js';
import {
	type UmbSubmittableWorkspaceContext,
	UmbSubmittableWorkspaceContextBase,
	UmbWorkspaceRouteManager,
	UmbWorkspaceIsNewRedirectController,
	type UmbRoutableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbWebhookWorkspaceContext
	extends UmbSubmittableWorkspaceContextBase<UmbWebhookDetailModel>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	public readonly repository: UmbWebhookDetailRepository = new UmbWebhookDetailRepository(this);

	#data = new UmbObjectState<UmbWebhookDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();

	readonly unique = this.#data.asObservablePart((data) => data?.unique);
	readonly name = this.#data.asObservablePart((data) => data?.name);

	readonly routes = new UmbWorkspaceRouteManager(this);

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.Webhook');

		this.routes.setRoutes([
			{
				path: 'create',
				component: UmbWebhookWorkspaceEditorElement,
				setup: async () => {
					this.create();

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
					this.removeUmbControllerByAlias('isNewRedirectController');
					this.load(info.match.params.unique);
				},
			},
		]);
	}

	protected resetState(): void {
		super.resetState();
		this.#data.setValue(undefined);
	}

	async load(unique: string) {
		this.resetState();
		const { data } = await this.repository.requestByUnique(unique);
		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}
	}

	async create() {
		this.resetState();
		const { data } = await this.repository.createScaffold();
		if (!data) return;
		this.setIsNew(true);
		this.#data.update(data);
		return { data };
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityType() {
		return 'webhook';
	}

	getUnique() {
		return this.#data.getValue()?.unique;
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	setEvents(events: string[]) {
		this.#data.update({ events });
	}

	setHeaders(headers: { [key: string]: string }) {
		this.#data.update({ headers });
	}

	setTypes(types: string[]) {
		this.#data.update({ contentTypes: types });
	}

	setUrl(url: string) {
		this.#data.update({ url });
	}

	setEnabled(enabled: boolean) {
		this.#data.update({ enabled });
	}

	async submit() {
		const newData = this.getData();
		if (!newData) {
			throw new Error('No data to submit');
		}

		if (this.getIsNew()) {
			const { error } = await this.repository.create(newData);
			if (error) {
				throw new Error(error.message);
			}
			this.setIsNew(false);
		} else {
			const { error } = await this.repository.save(newData);
			if (error) {
				throw new Error(error.message);
			}
		}
	}

	destroy(): void {
		this.#data.destroy();
		super.destroy();
	}
}

export { UmbWebhookWorkspaceContext as api };
