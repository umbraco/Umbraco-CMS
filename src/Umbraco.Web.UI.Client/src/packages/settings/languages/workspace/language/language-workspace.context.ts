import { UmbLanguageRepository } from '../../repository/language.repository.js';
import { UmbEntityWorkspaceContextInterface, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { ApiError, LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbLanguageWorkspaceContext
	extends UmbWorkspaceContext<UmbLanguageRepository, LanguageResponseModel>
	implements UmbEntityWorkspaceContextInterface
{
	#data = new UmbObjectState<LanguageResponseModel | undefined>(undefined);
	data = this.#data.asObservable();

	// TODO: this is a temp solution to bubble validation errors to the UI
	#validationErrors = new UmbObjectState<any | undefined>(undefined);
	validationErrors = this.#validationErrors.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, new UmbLanguageRepository(host));
	}

	async load(isoCode: string) {
		const { data } = await this.repository.requestByIsoCode(isoCode);
		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}
	}

	async create() {
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
	getEntityId() {
		return this.#data.getValue()?.isoCode;
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	setCulture(isoCode: string) {
		this.#data.update({ isoCode });
	}

	setMandatory(isMandatory: boolean) {
		this.#data.update({ isMandatory });
	}

	setDefault(isDefault: boolean) {
		this.#data.update({ isDefault });
	}

	setFallbackCulture(isoCode: string) {
		this.#data.update({ fallbackIsoCode: isoCode });
	}

	// TODO: this is a temp solution to bubble validation errors to the UI
	setValidationErrors(errorMap: any) {
		// TODO: I can't use the update method to set the value to undefined
		this.#validationErrors.next(errorMap);
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
		this.#data.complete();
	}
}
