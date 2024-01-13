import { UmbServerPathUniqueSerializer, appendFileExtensionIfNeeded } from '@umbraco-cms/backoffice/server-file-system';
import { type UmbPartialViewDetailModel } from '../types.js';
import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from '../entity.js';
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

	async createScaffold(parentUnique: string | null, preset?: Partial<UmbPartialViewDetailModel>) {
		const data: UmbPartialViewDetailModel = {
			entityType: UMB_PARTIAL_VIEW_ENTITY_TYPE,
			parentUnique,
			unique: '',
			path: '',
			name: '',
			content: '',
			...preset,
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
			parent: parentPath ? { path: parentPath } : null,
			name: appendFileExtensionIfNeeded(partialView.name, '.cshtml'),
			content: partialView.content,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			PartialViewResource.postPartialView({
				requestBody,
			}),
		);

		if (data) {
			const newPath = decodeURIComponent(data);
			const newPathUnique = this.#serverPathUniqueSerializer.toUnique(newPath);
			return this.read(newPathUnique);
		}

		return { error };
	}

	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const path = this.#serverPathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Path is missing');

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			PartialViewResource.getPartialViewByPath({ path: encodeURIComponent(path) }),
		);

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
		if (!path) throw new Error('Path is missing');

		const requestBody: UpdatePartialViewRequestModel = {
			content: data.content,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			PartialViewResource.putPartialViewByPath({
				path: encodeURIComponent(path),
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
		if (!path) throw new Error('Path is missing');

		return tryExecuteAndNotify(
			this.#host,
			PartialViewResource.deletePartialViewByPath({
				path: encodeURIComponent(path),
			}),
		);
	}
}
