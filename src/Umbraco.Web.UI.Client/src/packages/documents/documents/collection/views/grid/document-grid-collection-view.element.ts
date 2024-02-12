import type { UmbDocumentCollectionFilterModel } from '../../types.js';
import type { UmbDocumentTreeItemModel } from '../../../tree/types.js';
import { css, html, nothing, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_DEFAULT_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';

@customElement('umb-document-grid-collection-view')
export class UmbDocumentGridCollectionViewElement extends UmbLitElement {
	@state()
	private _items: Array<UmbDocumentTreeItemModel> = [];

	@state()
	private _selection: Array<string | null> = [];

	@state()
	private _loading = false;

	#collectionContext?: UmbDefaultCollectionContext<UmbDocumentTreeItemModel, UmbDocumentCollectionFilterModel>;

	constructor() {
		super();

		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.observe(
				this.#collectionContext.selection.selection,
				(selection) => (this._selection = selection),
				'umbCollectionSelectionObserver',
			);
			this.observe(this.#collectionContext.items, (items) => (this._items = items), 'umbCollectionItemsObserver');
		});
	}

	//TODO How should we handle url stuff?
	private _handleOpenCard(id: string) {
		//TODO this will not be needed when cards works as links with href
		history.pushState(null, '', 'section/content/workspace/document/edit/' + id);
	}

	#onSelect(item: UmbDocumentTreeItemModel) {
		this.#collectionContext?.selection.select(item.unique ?? '');
	}

	#onDeselect(item: UmbDocumentTreeItemModel) {
		this.#collectionContext?.selection.deselect(item.unique ?? '');
	}

	render() {
		if (this._loading) nothing;
		return html`
			<div id="document-grid">
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderCard(item),
				)}
			</div>
		`;
	}

	#renderCard(item: UmbDocumentTreeItemModel) {
		return html`
			<uui-card-content-node
				.name=${item.name ?? 'Unnamed Document'}
				selectable
				?select-only=${this._selection.length > 0}
				?selected=${this.#collectionContext?.selection.isSelected(item.unique ?? '')}
				@open=${() => this._handleOpenCard(item.unique ?? '')}
				@selected=${() => this.#onSelect(item)}
				@deselected=${() => this.#onDeselect(item)}>
				<uui-icon slot="icon" name=${item.documentType.icon}></uui-icon>
			</uui-card-content-node>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}

			#document-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
				gap: var(--uui-size-space-4);
			}

			uui-card-content-node {
				width: 100%;
				height: 180px;
			}
		`,
	];
}

export default UmbDocumentGridCollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-grid-collection-view': UmbDocumentGridCollectionViewElement;
	}
}
