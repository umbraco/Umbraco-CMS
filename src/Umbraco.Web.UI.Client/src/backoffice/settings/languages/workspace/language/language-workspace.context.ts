import { UmbLanguageRepository } from '../../repository/language.repository';
import { UmbWorkspaceContext } from '../../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbEntityWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import type { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { ObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export class UmbLanguageWorkspaceContext
	extends UmbWorkspaceContext<UmbLanguageRepository, LanguageResponseModel>
	implements UmbEntityWorkspaceContextInterface
{
	#data = new ObjectState<LanguageResponseModel | undefined>(undefined);
	data = this.#data.asObservable();

	// TODO: this is a temp solution to bubble validation errors to the UI
	#validationErrors = new ObjectState<any | undefined>(undefined);
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

	async createScaffold() {
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
	getEntityKey() {
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
		throw new Error('Save method not implemented.');
	}

	destroy(): void {
		this.#data.complete();
	}
}
