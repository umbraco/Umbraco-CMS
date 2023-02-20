import { UmbDocumentWorkspaceContext } from '../../../../documents/documents/workspace/document-workspace.context';
//import { DocumentModel } from '@umbraco-cms/backend-api';
import { UmbContextConsumerController, UmbContextProviderController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { NumberState, StringState } from '@umbraco-cms/observable-api';

//type EntityType = DocumentModel;

export class UmbVariantContentContext {
	#host: UmbControllerHostInterface;

	#workspaceContext?: UmbDocumentWorkspaceContext;

	#index = new NumberState(undefined);
	index = this.#index.asObservable();

	#culture = new StringState<string | null | undefined>(undefined);
	culture = this.#culture.asObservable();

	#segment = new StringState<string | null | undefined>(undefined);
	segment = this.#segment.asObservable();

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;

		new UmbContextProviderController(host, 'umbVariantContext', this);

		// How do we ensure this connects to a document workspace context? and not just any other context? (We could start providing workspace contexts twice, under the general name and under a specific name)
		// TODO: Figure out if this is the best way to consume the context or if it can be strongly typed with an UmbContextToken
		new UmbContextConsumerController(host, 'umbWorkspaceContext', (context) => {
			this.#workspaceContext = context as UmbDocumentWorkspaceContext;
		});
	}

	/*
	public getSplitViewIndex() {
		return this.#index.getValue();
	}
	*/
	public setSplitViewIndex(index: number) {
		this.#index.next(index);
	}

	/**
	 *
	 * concept this clas could have methods to set and get the culture and segment of the active variant? just by using the index.
	 */

	/*
	public destroy(): void {

	}
	*/
}
