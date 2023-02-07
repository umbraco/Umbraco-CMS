import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbMediaRepository } from '../repository/media.repository';
import type { UmbWorkspaceEntityContextInterface } from '../../../shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { appendToFrozenArray, ObjectState } from '@umbraco-cms/observable-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import type { MediaDetails } from '@umbraco-cms/models';

type EntityType = MediaDetails;
export class UmbMediaWorkspaceContext
	extends UmbWorkspaceContext
	implements UmbWorkspaceEntityContextInterface<EntityType | undefined>
{
	#isNew = false;
	#host: UmbControllerHostInterface;
	#detailRepository: UmbMediaRepository;

	#data = new ObjectState<EntityType | undefined>(undefined);
	data = this.#data.asObservable();
	name = this.#data.getObservablePart((data) => data?.name);

	constructor(host: UmbControllerHostInterface) {
		super(host);
		this.#host = host;
		this.#detailRepository = new UmbMediaRepository(this.#host);
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityKey() {
		return this.getData()?.key || '';
	}

	getEntityType() {
		return 'media';
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	setPropertyValue(alias: string, value: unknown) {
		const entry = { alias: alias, value: value };

		const currentData = this.#data.value;
		if (currentData) {
			const newDataSet = appendToFrozenArray(currentData.data, entry, (x) => x.alias);

			this.#data.update({ data: newDataSet });
		}
	}

	async load(entityKey: string) {
		const { data } = await this.#detailRepository.requestDetails(entityKey);
		if (data) {
			this.#isNew = false;
			this.#data.next(data);
		}
	}

	async createScaffold(parentKey: string | null) {
		const { data } = await this.#detailRepository.createDetailsScaffold(parentKey);
		if (!data) return;
		this.#isNew = true;
		this.#data.next(data);
	}

	async save() {
		if (!this.#data.value) return;
		if (this.#isNew) {
			await this.#detailRepository.createDetail(this.#data.value);
		} else {
			await this.#detailRepository.saveDetail(this.#data.value);
		}
		// If it went well, then its not new anymore?.
		this.#isNew = false;
	}

	async delete(key: string) {
		await this.#detailRepository.delete(key);
	}

	public destroy(): void {
		this.#data.complete();
	}
}
