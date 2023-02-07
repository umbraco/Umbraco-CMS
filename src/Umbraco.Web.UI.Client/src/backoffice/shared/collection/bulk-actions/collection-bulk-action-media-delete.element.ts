import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { map } from 'rxjs';
import { repeat } from 'lit-html/directives/repeat.js';
import { UmbCollectionContext, UMB_COLLECTION_CONTEXT_TOKEN } from '../collection.context';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '../../../../core/modal';
import { UmbLitElement } from '@umbraco-cms/element';
import type { ManifestCollectionBulkAction, MediaDetails } from '@umbraco-cms/models';

@customElement('umb-collection-bulk-action-media-delete')
export class UmbCollectionBulkActionDeleteElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	// TODO: make a UmbCollectionContextMedia:
	#collectionContext?: UmbCollectionContext<any, any>;

	public manifest?: ManifestCollectionBulkAction;

	#modalService?: UmbModalService;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT_TOKEN, (context) => {
			this.#collectionContext = context;
		});

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#modalService = instance;
		});
	}

	#handleClick(event: Event) {
		// TODO: Revisit this subscription nightmare, can we make this simpler, avoid subscribing to the selection?

		const selectionSubscription = this.#collectionContext?.selection.subscribe((selection) => {
			const dataSubscription = this.#collectionContext?.data
				.pipe(map((items) => items.filter((item) => item.key && selection.includes(item.key))))
				.subscribe((items: Array<any>) => {
					const modalHandler = this.#modalService?.confirm({
						headline: `Deleting ${selection.length} items`,
						content: html`
							This will delete the following files:
							<ul style="list-style-type: none; padding: 0; margin: 0; margin-top: var(--uui-size-space-2);">
								${repeat(
									items,
									(item) => item.key,
									(item) => html`<li style="font-weight: bold;">${item.name}</li>`
								)}
							</ul>
						`,
						color: 'danger',
						confirmLabel: 'Delete',
					});
					modalHandler?.onClose().then(({ confirmed }) => {
						selectionSubscription?.unsubscribe();
						dataSubscription?.unsubscribe();

						if (confirmed) {
							this.#collectionContext?.trash(selection);
							this.#collectionContext?.clearSelection();
						}
					});
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
		'umb-collection-bulk-action-media-delete': UmbCollectionBulkActionDeleteElement;
	}
}
