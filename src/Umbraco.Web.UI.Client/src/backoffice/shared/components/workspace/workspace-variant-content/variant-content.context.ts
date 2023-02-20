import {
	ActiveVariant,
	UmbDocumentWorkspaceContext,
} from '../../../../documents/documents/workspace/document-workspace.context';
//import { DocumentModel } from '@umbraco-cms/backend-api';
import { UmbContextConsumerController, UmbContextProviderController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { NumberState, ObjectState, UmbObserverController } from '@umbraco-cms/observable-api';
import { DocumentVariantModel } from '@umbraco-cms/backend-api';
import { UmbWorkspaceVariantableEntityContextInterface } from '../workspace-context/workspace-variantable-entity-context.interface';
import { UmbWorkspaceVariantPropertySetContext } from '../workspace-context/workspace-variant-property-set.context';
import { UmbVariantId } from 'src/backoffice/shared/variants/variant-id.class';

//type EntityType = DocumentModel;

export class UmbVariantContentContext {
	#host: UmbControllerHostInterface;

	#workspaceContext?: UmbDocumentWorkspaceContext;

	#index = new NumberState(undefined);
	index = this.#index.asObservable();

	#currentVariant = new ObjectState<DocumentVariantModel | undefined>(undefined);
	currentVariant = this.#currentVariant.asObservable();

	name = this.#currentVariant.getObservablePart((x) => x?.name);
	culture = this.#currentVariant.getObservablePart((x) => x?.culture);
	segment = this.#currentVariant.getObservablePart((x) => x?.segment);

	private _variantObserver?: UmbObserverController<ActiveVariant>;

	private _variantId?: UmbVariantId;

	#propertySetContext?: UmbWorkspaceVariantPropertySetContext;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;

		new UmbContextProviderController(host, 'umbVariantContext', this);

		// How do we ensure this connects to a document workspace context? and not just any other context? (We could start providing workspace contexts twice, under the general name and under a specific name)
		// TODO: Figure out if this is the best way to consume the context or if it can be strongly typed with an UmbContextToken
		new UmbContextConsumerController(host, 'umbWorkspaceContext', (context) => {
			this.#workspaceContext = context as UmbDocumentWorkspaceContext;
			this._providePropertySetContext();
			this._observeVariant();
		});

		this.#index.subscribe(() => {
			this._observeVariant();
		});
	}

	private _providePropertySetContext() {
		if (!this.#propertySetContext || !this.#workspaceContext || !this._variantId) return;
		this.#propertySetContext = new UmbWorkspaceVariantPropertySetContext(
			this.#host,
			this.#workspaceContext,
			this._variantId
		);
	}

	private _observeVariant() {
		if (!this.#workspaceContext) return;

		const index = this.#index.getValue();
		if (index === undefined) return;

		this._variantObserver?.destroy();
		this._variantObserver = new UmbObserverController(
			this.#host,
			this.#workspaceContext.activeVariantInfoByIndex(index),
			async (activeVariantInfo) => {
				this._variantId = activeVariantInfo.variantId;
				const currentVariant = await this.#workspaceContext?.getVariant(this._variantId);
				this.#currentVariant.next(currentVariant);
				this._providePropertySetContext();
			},
			'_observeVariant'
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

	public setName(newName: string) {
		if (!this.#workspaceContext || !this._variantId) return;
		this.#workspaceContext.setName(newName, this._variantId);
	}

	/**
	 *
	 * concept this class could have methods to set and get the culture and segment of the active variant? just by using the index.
	 */

	/*
	public destroy(): void {

	}
	*/
}
