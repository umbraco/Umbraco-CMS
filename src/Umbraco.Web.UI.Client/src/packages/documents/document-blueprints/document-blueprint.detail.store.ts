import type { DocumentBlueprintDetails } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbDocumentBlueprintStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Document Blueprints
 */
export class UmbDocumentBlueprintStore extends UmbStoreBase {
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_DOCUMENT_BLUEPRINT_STORE_CONTEXT_TOKEN.toString(),
			// TODO: use the right type:

			new UmbArrayState<DocumentBlueprintDetails>([], (x) => x.id)
		);
	}

	/**
	 * @description - Request a Data Type by id. The Data Type is added to the store and is returned as an Observable.
	 * @param {string} id
	 * @return {*}  {(Observable<DocumentBlueprintDetails | undefined>)}
	 * @memberof UmbDocumentBlueprintStore
	 */
	getById(id: string) {
		// TODO: use backend cli when available.
		fetch(`/umbraco/management/api/v1/document-blueprint/details/${id}`)
			.then((res) => res.json())
			.then((data) => {
				this._data.append(data);
			});

		return this._data.asObservablePart((documents) => documents.find((document) => document.id === id));
	}

	getScaffold(entityType: string, parentId: string | null) {
		return {} as DocumentBlueprintDetails;
	}

	// TODO: make sure UI somehow can follow the status of this action.
	/**
	 * @description - Save a DocumentBlueprint.
	 * @param {Array<DocumentBlueprintDetails>} Dictionaries
	 * @memberof UmbDocumentBlueprintStore
	 * @return {*}  {Promise<void>}
	 */
	save(data: DocumentBlueprintDetails[]) {
		// fetch from server and update store
		// TODO: use Fetcher API.
		let body: string;

		try {
			body = JSON.stringify(data);
		} catch (error) {
			console.error(error);
			return Promise.reject();
		}

		// TODO: use backend cli when available.
		return fetch('/umbraco/management/api/v1/document-blueprint/save', {
			method: 'POST',
			body: body,
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((res) => res.json())
			.then((data: Array<DocumentBlueprintDetails>) => {
				this._data.append(data);
			});
	}

	// TODO: How can we avoid having this in both stores?
	/**
	 * @description - Delete a Data Type.
	 * @param {string[]} ids
	 * @memberof UmbDocumentBlueprintStore
	 * @return {*}  {Promise<void>}
	 */
	async delete(ids: string[]) {
		// TODO: use backend cli when available.
		await fetch('/umbraco/backoffice/document-blueprint/delete', {
			method: 'POST',
			body: JSON.stringify(ids),
			headers: {
				'Content-Type': 'application/json',
			},
		});

		this._data.remove(ids);
	}
}

export const UMB_DOCUMENT_BLUEPRINT_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentBlueprintStore>(
	'UmbDocumentBlueprintStore'
);
