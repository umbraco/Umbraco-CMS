import { UmbLanguageRepository } from '../../repository/language.repository';
import type { LanguageModel } from '@umbraco-cms/backend-api';
import { ObjectState } from '@umbraco-cms/observable-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbWorkspaceContext } from 'src/backoffice/shared/components/workspace/workspace-context/workspace-context';

export class UmbLanguageWorkspaceContext extends UmbWorkspaceContext {
	#host: UmbControllerHostInterface;
	#data = new ObjectState<LanguageModel | undefined>(undefined);
	#languageRepository: UmbLanguageRepository;
	#isNew = false;

	constructor(host: UmbControllerHostInterface) {
		super(host);
		this.#host = host;
		this.#languageRepository = new UmbLanguageRepository(this.#host);
	}

	async load(isoCode: string) {
		const { data } = await this.#languageRepository.requestByIsoCode(isoCode);
		if (data) {
			this.#isNew = false;
			this.#data.update(data);
		}
	}

	async createScaffold() {
		const { data } = await this.#languageRepository.createDetailsScaffold();
		if (!data) return;
		this.#isNew = true;
		this.#data.update(data);
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityType() {
		return 'language';
	}

	public destroy(): void {
		this.#data.complete();
	}
}
