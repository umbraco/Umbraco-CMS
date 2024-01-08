import { type UmbPartialViewDetailModel } from '../types.js';
import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from '../entity.js';
import { UmbServerPathUniqueSerializer, appendFileExtensionIfNeeded } from '../../utils/index.js';
import {
	CreatePartialViewRequestModel,
	PartialViewResource,
	UpdatePartialViewRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbPartialViewDetailServerDataSource implements UmbDetailDataSource<UmbPartialViewDetailModel> {
	#host: UmbControllerHost;
	#serverPathUniqueSerializer = new UmbServerPathUniqueSerializer();

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async createScaffold(parentUnique: string | null) {
		const data: UmbPartialViewDetailModel = {
			entityType: UMB_PARTIAL_VIEW_ENTITY_TYPE,
			parentUnique,
			unique: '',
			path: '',
			name: '',
			content: '',
		};

		return { data };
	}

	async create(partialView: UmbPartialViewDetailModel) {
		if (!partialView) throw new Error('Data is missing');
		if (!partialView.parentUnique === undefined) throw new Error('Parent Unique is missing');
		if (!partialView.name) throw new Error('Name is missing');

		const parentPath = this.#serverPathUniqueSerializer.toServerPath(partialView.parentUnique);

		// TODO: make data mapper to prevent errors
		const requestBody: CreatePartialViewRequestModel = {
			parent: {
				path: parentPath,
			},
			name: appendFileExtensionIfNeeded(partialView.name, '.cshtml'),
			content: partialView.content,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			PartialViewResource.postPartialView({
				requestBody,
			}),
		);

		if (error) {
			return { error };
		}

		// We have to fetch the data again. The server can have modified the data after creation
		// TODO: revisit when location header is added
		const createdPartialViewPath = parentPath ? parentPath + '/' + requestBody.name : requestBody.name;
		const createdPartialViewUnique = this.#serverPathUniqueSerializer.toUnique(createdPartialViewPath);

		return this.read(createdPartialViewUnique);
	}

	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const path = this.#serverPathUniqueSerializer.toServerPath(unique);

		const { data, error } = await tryExecuteAndNotify(this.#host, PartialViewResource.getPartialViewByPath({ path }));

		if (error || !data) {
			return { error };
		}

		const partialView: UmbPartialViewDetailModel = {
			entityType: UMB_PARTIAL_VIEW_ENTITY_TYPE,
			unique: this.#serverPathUniqueSerializer.toUnique(data.path),
			parentUnique: data.parent ? this.#serverPathUniqueSerializer.toUnique(data.parent.path) : null,
			path: data.path,
			name: data.name,
			content: data.content,
		};

		return { data: partialView };
	}

	async update(data: UmbPartialViewDetailModel) {
		if (!data.unique) throw new Error('Unique is missing');

		const path = this.#serverPathUniqueSerializer.toServerPath(data.unique);

		const requestBody: UpdatePartialViewRequestModel = {
			content: data.content,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			PartialViewResource.putPartialViewByPath({
				path,
				requestBody,
			}),
		);

		if (error) {
			return { error };
		}

		return this.read(data.unique);
	}

	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const path = this.#serverPathUniqueSerializer.toServerPath(unique);

		return tryExecuteAndNotify(
			this.#host,
			PartialViewResource.deletePartialViewByPath({
				path,
			}),
		);
	}
}
