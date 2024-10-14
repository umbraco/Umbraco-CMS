import type { UmbRelationTypeDetailModel } from '../../types.js';
import { UMB_RELATION_TYPE_ENTITY_TYPE } from '../../entity.js';
import { RelationTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbReadDetailDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the Relation Type that fetches data from the server
 * @class UmbRelationTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbRelationTypeDetailServerDataSource implements UmbReadDetailDataSource<UmbRelationTypeDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbRelationTypeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbRelationTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches a Relation Type with the given id from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbRelationTypeServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			RelationTypeService.getRelationTypeById({ id: unique }),
		);

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const relationType: UmbRelationTypeDetailModel = {
			alias: data.alias || '',
			child: data.childObject
				? {
						objectType: {
							unique: data.childObject.id,
							name: data.childObject.name || '',
						},
					}
				: null,
			entityType: UMB_RELATION_TYPE_ENTITY_TYPE,
			isBidirectional: data.isBidirectional,
			isDependency: data.isDependency,
			name: data.name,
			parent: data.parentObject
				? {
						objectType: {
							unique: data.parentObject.id,
							name: data.parentObject.name || '',
						},
					}
				: null,
			unique: data.id,
		};

		return { data: relationType };
	}
}
