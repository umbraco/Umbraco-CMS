import { UmbMediaTypeRepository } from '../repository/media-type.repository.js';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbSaveableWorkspaceContextInterface, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { MediaTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

type EntityType = MediaTypeResponseModel;
export class UmbMediaTypeWorkspaceContext
	extends UmbWorkspaceContext<UmbMediaTypeRepository, EntityType>
	implements UmbSaveableWorkspaceContextInterface<EntityType | undefined>
{
	#data = new UmbObjectState<EntityType | undefined>(undefined);
	data = this.#data.asObservable();
	#getDataPromise?: Promise<any>;

	name = this.#data.asObservablePart((data) => data?.name);
	id = this.#data.asObservablePart((data) => data?.id);
	alias = this.#data.asObservablePart((data) => data?.alias);
	description = this.#data.asObservablePart((data) => data?.description);
	icon = this.#data.asObservablePart((data) => data?.icon);

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.MediaType', new UmbMediaTypeRepository(host));
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityId() {
		return this.getData()?.id || '';
	}

	getEntityType() {
		return 'media-type';
	}

	updateProperty<PropertyName extends keyof EntityType>(propertyName: PropertyName, value: EntityType[PropertyName]) {
		this.#data.update({ [propertyName]: value });
	}

	async load(id: string) {
		this.#getDataPromise = this.repository.requestById(id);
		const { data } = await this.#getDataPromise;
		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}
	}

	async create() {
		const { data } = await this.repository.createScaffold();
		if (!data) return;

		this.setIsNew(true);
		this.#data.next(data);
	}

	async save() {
		const id = await firstValueFrom(this.id);

		if (!this.#data.value || !id) return;

		await this.repository.save(id, this.#data.value);
		this.setIsNew(false);
	}

	public destroy(): void {
		this.#data.complete();
	}
}

export const UMB_MEDIA_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbMediaTypeWorkspaceContext
>(
	'UmbWorkspaceContext',
	(context): context is UmbMediaTypeWorkspaceContext => context.getEntityType?.() === 'media-type',
);
