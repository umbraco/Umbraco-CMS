import type { DocumentBlueprintDetails } from '@umbraco-cms/backoffice/models';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { ArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

/**
 * @export
 * @class UmbDocumentBlueprintStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Document Blueprints
 */
export class UmbDocumentBlueprintStore extends UmbStoreBase {
	// TODO: use the right type:
	#data = new ArrayState<DocumentBlueprintDetails>([], (x) => x.key);

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DOCUMENT_BLUEPRINT_STORE_CONTEXT_TOKEN.toString());
	}

	/**
	 * @description - Request a Data Type by key. The Data Type is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<DocumentBlueprintDetails | undefined>)}
	 * @memberof UmbDocumentBlueprintStore
	 */
	getByKey(key: string) {
		// TODO: use backend cli when available.
		fetch(`/umbraco/management/api/v1/document-blueprint/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.#data.append(data);
			});

		return this.#data.getObservablePart((documents) => documents.find((document) => document.key === key));
	}

	getScaffold(entityType: string, parentKey: string | null) {
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
				this.#data.append(data);
			});
	}

	// TODO: How can we avoid having this in both stores?
	/**
	 * @description - Delete a Data Type.
	 * @param {string[]} keys
	 * @memberof UmbDocumentBlueprintStore
	 * @return {*}  {Promise<void>}
	 */
	async delete(keys: string[]) {
		// TODO: use backend cli when available.
		await fetch('/umbraco/backoffice/document-blueprint/delete', {
			method: 'POST',
			body: JSON.stringify(keys),
			headers: {
				'Content-Type': 'application/json',
			},
		});

		this.#data.remove(keys);
	}
}

export const UMB_DOCUMENT_BLUEPRINT_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentBlueprintStore>(
	'UmbDocumentBlueprintStore'
);
