import {
	ActiveVariant,
	UmbDocumentWorkspaceContext,
} from '../../../../documents/documents/workspace/document-workspace.context';
//import { DocumentModel } from '@umbraco-cms/backend-api';
import { UmbContextConsumerController, UmbContextProviderController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { NumberState, ObjectState, UmbObserverController } from '@umbraco-cms/observable-api';
import { DocumentVariantModel } from '@umbraco-cms/backend-api';

//type EntityType = DocumentModel;

export class UmbVariantContentContext {
	#host: UmbControllerHostInterface;

	#workspaceContext?: UmbDocumentWorkspaceContext;

	#index = new NumberState(undefined);
	index = this.#index.asObservable();

	#currentVariant = new ObjectState<DocumentVariantModel | undefined>(undefined);
	currentVariant = this.#currentVariant.asObservable();

	culture = this.#currentVariant.getObservablePart((x) => x?.culture);
	segment = this.#currentVariant.getObservablePart((x) => x?.segment);

	private _variantObserver?: UmbObserverController<ActiveVariant>;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;

		new UmbContextProviderController(host, 'umbVariantContext', this);

		// How do we ensure this connects to a document workspace context? and not just any other context? (We could start providing workspace contexts twice, under the general name and under a specific name)
		// TODO: Figure out if this is the best way to consume the context or if it can be strongly typed with an UmbContextToken
		new UmbContextConsumerController(host, 'umbWorkspaceContext', (context) => {
			this.#workspaceContext = context as UmbDocumentWorkspaceContext;
			this._observeVariant();
		});

		this.#index.subscribe(() => {
			this._observeVariant();
		});
	}

	private _observeVariant() {
		if (!this.#workspaceContext) return;

		const index = this.#index.getValue();
		if (index === undefined) return;

		this._variantObserver?.destroy();
		this._variantObserver = new UmbObserverController(
			this.#host,
			this.#workspaceContext.activeVariantWithIndex(index),
			(variant) => {
				this.#currentVariant.next(variant);
			}
		);
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
