import {
	type UmbSubmittableWorkspaceContext,
	UmbSubmittableWorkspaceContextBase,
	UmbWorkspaceRouteManager,
	UmbWorkspaceIsNewRedirectController,
	type UmbRoutableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbLanguageDetailRepository } from '../../repository/index.js';
import type { UmbLanguageDetailModel } from '../../types.js';
import { UmbLanguageWorkspaceEditorElement } from './language-workspace-editor.element.js';

export class UmbLanguageWorkspaceContext
	extends UmbSubmittableWorkspaceContextBase<UmbLanguageDetailModel>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	public readonly repository: UmbLanguageDetailRepository = new UmbLanguageDetailRepository(this);

	#data = new UmbObjectState<UmbLanguageDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();

	readonly unique = this.#data.asObservablePart((data) => data?.unique);
	readonly name = this.#data.asObservablePart((data) => data?.name);

	// TODO: this is a temp solution to bubble validation errors to the UI
	#validationErrors = new UmbObjectState<any | undefined>(undefined);
	readonly validationErrors = this.#validationErrors.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.Language');

		this.routes.setRoutes([
			{
				path: 'create',
				component: UmbLanguageWorkspaceEditorElement,
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
				component: UmbLanguageWorkspaceEditorElement,
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
		return 'language';
	}

	// TODO: Convert to uniques:
	getUnique() {
		return this.#data.getValue()?.unique;
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	setCulture(unique: string) {
		this.#data.update({ unique });
	}

	setMandatory(isMandatory: boolean) {
		this.#data.update({ isMandatory });
	}

	setDefault(isDefault: boolean) {
		this.#data.update({ isDefault });
	}

	setFallbackCulture(unique: string) {
		this.#data.update({ fallbackIsoCode: unique });
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

export { UmbLanguageWorkspaceContext as api };
