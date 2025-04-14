import type {
	UmbMemberTypeCompositionCompatibleModel,
	UmbMemberTypeCompositionReferenceModel,
	UmbMemberTypeAvailableCompositionRequestModel,
} from '../../types.js';
import {
	type MemberTypeCompositionRequestModel,
	MemberTypeService,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbContentTypeCompositionDataSource } from '@umbraco-cms/backoffice/content-type';

/**
 * A data source for the Member Type Composition that fetches data from the server
 * @class UmbMemberTypeCompositionServerDataSource
 */
export class UmbMemberTypeCompositionServerDataSource
	implements
		UmbContentTypeCompositionDataSource<
			UmbMemberTypeCompositionReferenceModel,
			UmbMemberTypeCompositionCompatibleModel,
			UmbMemberTypeAvailableCompositionRequestModel
		>
{
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberTypeCompositionServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberTypeCompositionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}
	/**
	 * Fetches the compatible compositions for a document type from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbMemberTypeCompositionServerDataSource
	 */
	async getReferences(unique: string) {
		const response = await tryExecute(
			this.#host,
			MemberTypeService.getMemberTypeByIdCompositionReferences({ path: { id: unique } }),
		);
		const error = response.error;
		const data: Array<UmbMemberTypeCompositionReferenceModel> | undefined = response.data?.map((reference) => {
			return {
				unique: reference.id,
				icon: reference.icon,
				name: reference.name,
			};
		});

		return { data, error };
	}
	/**
	 * Updates the compositions for a document type on the server
	 * @param {MemberTypeCompositionRequestModel} body
	 * @param args
	 * @returns {*}
	 * @memberof UmbMemberTypeCompositionServerDataSource
	 */
	async availableCompositions(args: UmbMemberTypeAvailableCompositionRequestModel) {
		const body: MemberTypeCompositionRequestModel = {
			id: args.unique,
			currentCompositeIds: args.currentCompositeUniques,
			currentPropertyAliases: args.currentPropertyAliases,
		};

		const response = await tryExecute(this.#host, MemberTypeService.postMemberTypeAvailableCompositions({ body }));
		const error = response.error;
		const data: Array<UmbMemberTypeCompositionCompatibleModel> | undefined = response.data?.map((composition) => {
			return {
				unique: composition.id,
				name: composition.name,
				icon: composition.icon,
				folderPath: composition.folderPath,
				isCompatible: composition.isCompatible,
			};
		});

		return { data, error };
	}
}
