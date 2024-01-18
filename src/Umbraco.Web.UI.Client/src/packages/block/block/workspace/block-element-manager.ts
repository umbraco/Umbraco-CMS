import { UmbBlockDataType } from '../types.js';
import { UmbBlockElementPropertyDatasetContext } from './block-element-property-dataset.context.js';
import { UmbContentTypePropertyStructureManager } from '@umbraco-cms/backoffice/content-type';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';

export class UmbBlockElementManager extends UmbBaseController {
	//
	#data = new UmbObjectState<UmbBlockDataType | undefined>(undefined);
	#getDataPromise = new Promise<void>((resolve) => {
		this.#getDataResolver = resolve;
	});
	#getDataResolver!: () => void;

	readonly unique = this.#data.asObservablePart((data) => data?.udi);
	readonly contentTypeId = this.#data.asObservablePart((data) => data?.contentTypeKey);

	readonly structure = new UmbContentTypePropertyStructureManager(this, new UmbDocumentTypeDetailRepository(this));

	constructor(host: UmbControllerHost) {
		// TODO: Get Workspace Alias via Manifest.
		super(host);

		this.observe(this.contentTypeId, (id) => this.structure.loadType(id));
	}

	setData(data: UmbBlockDataType | undefined) {
		this.#data.setValue(data);
		this.#getDataResolver();
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityId() {
		return this.getData()?.udi;
	}

	getEntityType() {
		return 'element';
	}

	getContentTypeId() {
		return this.getData()?.contentTypeKey;
	}

	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		await this.#getDataPromise;

		return this.#data.asObservablePart((data) => data?.[propertyAlias] as ReturnType);
	}

	async getPropertyValue<ReturnType = unknown>(propertyAlias: string) {
		await this.#getDataPromise;

		return this.#data.getValue()?.[propertyAlias] as ReturnType;
	}

	async setPropertyValue(alias: string, value: unknown) {
		await this.#getDataPromise;

		this.#data.update({ [alias]: value });
	}

	public createPropertyDatasetContext(host: UmbControllerHost) {
		return new UmbBlockElementPropertyDatasetContext(host, this);
	}

	public destroy(): void {
		this.#data.destroy();
		this.structure.destroy();
		super.destroy();
	}
}

export default UmbBlockElementManager;
