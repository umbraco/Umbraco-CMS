import { UmbMediaTypeRepository } from '../repository/media-type.repository.js';
import { UmbSaveableWorkspaceContextInterface, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { MediaTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbMediaTypeWorkspaceContext
	extends UmbWorkspaceContext<UmbMediaTypeRepository, MediaTypeResponseModel>
	implements UmbSaveableWorkspaceContextInterface<MediaTypeResponseModel | undefined>
{
	#data = new UmbObjectState<MediaTypeResponseModel | undefined>(undefined);
	data = this.#data.asObservable();
	#getDataPromise?: Promise<any>;

	name = this.#data.asObservablePart((data) => data?.name);
	id = this.#data.asObservablePart((data) => data?.id);

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

	setName(name: string) {
		this.#data.update({ name });
	}

	setPropertyValue(alias: string, value: string) {
		// TODO => Implement setPropertyValue
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
		if (!this.#data.value) return;
		await this.repository.save(this.#data.value);
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
