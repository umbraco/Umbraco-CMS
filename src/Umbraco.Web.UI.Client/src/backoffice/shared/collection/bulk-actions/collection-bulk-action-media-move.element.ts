import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbCollectionContext, UMB_COLLECTION_CONTEXT_TOKEN } from '../collection.context';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '../../../../core/modal';
import { UmbMediaTreeStore, UMB_MEDIA_TREE_STORE_CONTEXT_TOKEN } from '../../../media/media/media.tree.store';
import { UmbLitElement } from '@umbraco-cms/element';
import type { ManifestCollectionBulkAction } from '@umbraco-cms/models';

@customElement('umb-collection-bulk-action-media-move')
export class UmbCollectionBulkActionMoveElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	// TODO: make a UmbCollectionContextMedia:
	#collectionContext?: UmbCollectionContext<any, any>;

	public manifest?: ManifestCollectionBulkAction;

	#modalService?: UmbModalService;
	#mediaTreeStore?: UmbMediaTreeStore;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT_TOKEN, (context) => {
			this.#collectionContext = context;
		});

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#modalService = instance;
		});

		this.consumeContext(UMB_MEDIA_TREE_STORE_CONTEXT_TOKEN, (instance) => {
			this.#mediaTreeStore = instance;
		});
	}

	#handleClick() {
		const selectionSubscription = this.#collectionContext?.selection.subscribe((selection) => {
			const modalHandler = this.#modalService?.mediaPicker({
				selection: [],
				multiple: false,
			});
			modalHandler?.onClose().then((data) => {
				if (selection.length > 0) {
					this.#mediaTreeStore?.move(selection, data.selection[0]);
				}
				selectionSubscription?.unsubscribe();
				this.#collectionContext?.clearSelection();
			});
		});
	}

	render() {
		// TODO: make a UmbCollectionContextMedia and use a deleteSelection method.
		return html`<uui-button
			@click=${this.#handleClick}
			label=${ifDefined(this.manifest?.meta.label)}
			color="default"
			look="secondary"></uui-button>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-bulk-action-media-move': UmbCollectionBulkActionMoveElement;
	}
}
