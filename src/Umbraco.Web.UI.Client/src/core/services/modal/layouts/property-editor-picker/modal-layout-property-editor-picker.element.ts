import { css, html, LitElement } from 'lit';
import { repeat } from 'lit/directives/repeat.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../context';

import type { Subscription } from 'rxjs';
import type { UmbModalHandler } from '../../modal-handler';
import type { PropertyEditor } from '../../../../../mocks/data/property-editor.data';
import type { UmbPropertyEditorStore } from '../../../../stores/property-editor.store';
import { UUIInputEvent } from '@umbraco-ui/uui';

export interface UmbModalPropertyEditorPickerData {
	selection?: Array<string>;
	multiple?: boolean;
	submitLabel?: string;
}

@customElement('umb-modal-layout-property-editor-picker')
export class UmbModalLayoutPropertyEditorPickerElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			#filter {
				width: 100%;
				margin-bottom: var(--uui-size-space-4);
			}

			#filter-icon {
				padding-left: var(--uui-size-space-2);
			}

			#item-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(70px, 1fr));
				margin: 0;
				padding: 0;
				grid-gap: var(--uui-size-space-4);
			}

			#item-grid .item {
				display: flex;
				align-items: flex-start;
				justify-content: center;
				list-style: none;
				height: 100%;
				border: 1px solid transparent;
			}

			#item-grid .item:hover {
				background: whitesmoke;
				cursor: pointer;
			}

			#item-grid .item[selected] button {
				background: var(--uui-color-selected);
				color: var(--uui-color-selected-contrast);
			}

			#item-grid .item button {
				background: none;
				border: none;
				cursor: pointer;
				padding: var(--uui-size-space-3);
				display: flex;
				align-items: center;
				flex-direction: column;
				justify-content: center;
				font-size: 0.8rem;
				height: 100%;
				width: 100%;
			}

			#item-grid .item .icon {
				font-size: 2em;
				margin-bottom: var(--uui-size-space-2);
			}
		`,
	];

	@property({ attribute: false })
	modalHandler?: UmbModalHandler;

	@property({ type: Object })
	data?: UmbModalPropertyEditorPickerData;

	@state()
	private _filteredPropertyEditors: Array<PropertyEditor> = [];

	@state()
	private _selection: Array<string> = [];

	@state()
	private _submitLabel = 'Select';

	private _propertyEditors: Array<PropertyEditor> = [];

	private _propertyEditorStore?: UmbPropertyEditorStore;
	private _propertyEditorsSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbPropertyEditorStore', (propertyEditorStore) => {
			this._propertyEditorStore = propertyEditorStore;
			this._observePropertyEditors();
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		this._selection = this.data?.selection ?? [];
		this._submitLabel = this.data?.submitLabel ?? this._submitLabel;
	}

	private _observePropertyEditors() {
		this._propertyEditorsSubscription = this._propertyEditorStore?.getAll().subscribe((propertyEditors) => {
			this._propertyEditors = propertyEditors;
			this._filteredPropertyEditors = propertyEditors;
		});
	}

	private _handleClick(propertyEditor: PropertyEditor) {
		if (this.data?.multiple) {
			this._multiSelect(propertyEditor.alias);
		} else {
			this._select(propertyEditor.alias);
		}
	}

	private _select(alias: string) {
		this._selection = [alias];
	}

	private _multiSelect(alias: string) {
		if (this._selection?.includes(alias)) {
			this._selection = this._selection.filter((item) => item !== alias);
		} else {
			this._selection = [...this._selection, alias];
		}
	}

	private _handleFilterInput(event: UUIInputEvent) {
		let query = (event.target.value as string) || '';
		query = query.toLowerCase();

		this._filteredPropertyEditors = !query
			? this._propertyEditors
			: this._propertyEditors.filter((propertyEditor) => {
					return (
						propertyEditor.name.toLowerCase().includes(query) || propertyEditor.alias.toLowerCase().includes(query)
					);
			  });
	}

	private _close() {
		this.modalHandler?.close();
	}

	private _submit() {
		this.modalHandler?.close({ selection: this._selection });
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._propertyEditorsSubscription?.unsubscribe();
	}

	render() {
		return html`
			<umb-editor-entity-layout headline="Select Property Editor UI">
				<uui-box> ${this._renderFilter()} ${this._renderGrid()} </uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
					<uui-button label="${this._submitLabel}" look="primary" color="positive" @click=${this._submit}></uui-button>
				</div>
			</umb-editor-entity-layout>
		`;
	}

	private _renderFilter() {
		return html` <uui-input
			id="filter"
			@input="${this._handleFilterInput}"
			placeholder="Type to filter..."
			label="Type to filter icons">
			<uui-icon name="search" slot="prepend" id="filter-icon"></uui-icon>
		</uui-input>`;
	}

	private _renderGrid() {
		return html`<ul id="item-grid">
			${repeat(
				this._filteredPropertyEditors,
				(propertyEditor) => propertyEditor.alias,
				(propertyEditor) => html` <li class="item" ?selected=${this._selection.includes(propertyEditor.alias)}>
					<button type="button" @click="${() => this._handleClick(propertyEditor)}">
						<uui-icon name="document" class="icon"></uui-icon>
						${propertyEditor.name}
					</button>
				</li>`
			)}
		</ul>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-property-editor-picker': UmbModalLayoutPropertyEditorPickerElement;
	}
}
