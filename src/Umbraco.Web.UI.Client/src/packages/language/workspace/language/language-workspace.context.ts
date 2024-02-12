import { UmbLanguageDetailRepository } from '../../repository/index.js';
import type { UmbLanguageDetailModel } from '../../types.js';
import {
	type UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import { ApiError } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbLanguageWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbLanguageDetailModel>
	implements UmbSaveableWorkspaceContextInterface
{
	public readonly repository: UmbLanguageDetailRepository = new UmbLanguageDetailRepository(this);

	#data = new UmbObjectState<UmbLanguageDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();

	// TODO: this is a temp solution to bubble validation errors to the UI
	#validationErrors = new UmbObjectState<any | undefined>(undefined);
	readonly validationErrors = this.#validationErrors.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.Language');
	}

	async load(unique: string) {
		const { data } = await this.repository.requestByUnique(unique);
		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}
	}

	async create() {
		const { data } = await this.repository.createScaffold(null);
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
	getEntityId() {
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

	// TODO: this is a temp solution to bubble validation errors to the UI
	setValidationErrors(errorMap: any) {
		// TODO: I can't use the update method to set the value to undefined
		this.#validationErrors.setValue(errorMap);
	}

	async save() {
		const data = this.getData();
		if (!data) return;

		if (this.getIsNew()) {
			const { error } = await this.repository.create(data);
			// TODO: this is temp solution to bubble validation errors to the UI
			if (error) {
				if (error instanceof ApiError && error.body.type === 'validation') {
					this.setValidationErrors?.(error.body.errors);
				}
			} else {
				this.setValidationErrors?.(undefined);
				// TODO: do not make it the buttons responsibility to set the workspace to not new.
				this.setIsNew(false);
			}
		} else {
			await this.repository.save(data);
			// TODO: Show validation errors as warnings?
		}
	}

	destroy(): void {
		this.#data.destroy();
	}
}

export const UMB_LANGUAGE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbLanguageWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbLanguageWorkspaceContext => context.getEntityType?.() === 'language',
);
