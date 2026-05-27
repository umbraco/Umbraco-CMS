import type { UmbElementFolderItemModel } from '../repository/item/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbItemDataResolver } from '@umbraco-cms/backoffice/entity-item';

/**
 * A controller for resolving data for an element folder item
 * @exports
 * @class UmbElementFolderItemDataResolver
 * @augments {UmbControllerBase}
 */
export class UmbElementFolderItemDataResolver extends UmbControllerBase implements UmbItemDataResolver {
	#data = new UmbObjectState<UmbElementFolderItemModel | undefined>(undefined);

	public readonly entityType = this.#data.asObservablePart((x) => x?.entityType);
	public readonly unique = this.#data.asObservablePart((x) => x?.unique);
	public readonly name = this.#data.asObservablePart((x) => x?.name);
	public readonly icon = this.#data.asObservablePart((x) => x?.icon);

	constructor(host: UmbControllerHost) {
		super(host);
	}

	getData(): UmbElementFolderItemModel | undefined {
		return this.#data.getValue();
	}

	setData(data: UmbElementFolderItemModel | undefined) {
		this.#data.setValue(data);
	}

	async getEntityType(): Promise<string | undefined> {
		return await this.observe(this.entityType).asPromise();
	}

	async getUnique(): Promise<string | undefined> {
		return await this.observe(this.unique).asPromise();
	}

	async getName(): Promise<string | undefined> {
		return await this.observe(this.name).asPromise();
	}

	async getIcon(): Promise<string | undefined> {
		return await this.observe(this.icon).asPromise();
	}
}
