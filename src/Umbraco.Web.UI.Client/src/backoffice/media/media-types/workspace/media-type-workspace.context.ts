import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbWorkspaceEntityContextInterface } from '../../../shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { UmbMediaTypeRepository } from '../repository/media-type.repository';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { ObjectState } from '@umbraco-cms/observable-api';
import type { MediaTypeDetails } from '@umbraco-cms/models';

type EntityType = MediaTypeDetails;
export class UmbWorkspaceMediaTypeContext
	extends UmbWorkspaceContext<UmbMediaTypeRepository>
	implements UmbWorkspaceEntityContextInterface<EntityType | undefined>
{
	#data = new ObjectState<MediaTypeDetails | undefined>(undefined);
	data = this.#data.asObservable();
	name = this.#data.getObservablePart((data) => data?.name);

	constructor(host: UmbControllerHostInterface) {
		super(host, new UmbMediaTypeRepository(host));
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityKey() {
		return this.getData()?.key || '';
	}

	getEntityType() {
		return 'media-type';
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	setPropertyValue(alias: string, value: string) {
		// TODO => Implement setPropertyValue
	}

	async load(entityKey: string) {
		const { data } = await this.repository.requestDetails(entityKey);
		if (data) {
			this.#data.next(data);
		}
	}

	async createScaffold() {
		const { data } = await this.repository.createScaffold();
		if (!data) return;
		this.setIsNew(true);
		this.#data.next(data);
	}

	async save() {
		if (!this.#data.value) return;
		await this.repository.save(this.#data.value);
		this.setIsNew(false);
	}

	public destroy(): void {
		this.#data.complete();
	}
}
