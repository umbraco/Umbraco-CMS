import { UmbLanguageRepository } from '../../repository/language.repository';
import { UmbWorkspaceContext } from '../../../../shared/components/workspace/workspace-context/workspace-context';
import type { LanguageModel } from '@umbraco-cms/backend-api';
import { ObjectState } from '@umbraco-cms/observable-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbLanguageWorkspaceContext extends UmbWorkspaceContext<UmbLanguageRepository> {
	#data = new ObjectState<LanguageModel | undefined>(undefined);
	data = this.#data.asObservable();

	// TODO: this is a temp solution to bubble validation errors to the UI
	#validationErrors = new ObjectState<any | undefined>(undefined);
	validationErrors = this.#validationErrors.asObservable();

	constructor(host: UmbControllerHostInterface) {
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
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityType() {
		return 'language';
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

	destroy(): void {
		this.#data.complete();
	}
}
