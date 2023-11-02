import { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UMB_COLLECTION_CONTEXT, UmbCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-collection-pagination')
export class UmbCollectionPaginationElement extends UmbLitElement {

  @state()
  _totalPages = 11;

  @state()
  _currentPage = 1;

	private _collectionContext?: UmbCollectionContext<any, any>;

	constructor() {
		super();
		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this._collectionContext = instance;
		});
	}

  #onChange (event: UUIPaginationEvent) { 
    console.log(event);
    console.log(event.target.current);
    this._collectionContext?.setPage(event.target.current);
  }

	render() {
    if (this._totalPages === 0) {
      return nothing;
    }

		return html`<uui-pagination .current=${this._currentPage} .total=${this._totalPages} @change=${this.#onChange}></uui-pagination>`;
	}

	static styles = [
		UmbTextStyles,
		css`
      :host {
        display: block;
      }
    `
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-pagination': UmbCollectionPaginationElement;
	}
}