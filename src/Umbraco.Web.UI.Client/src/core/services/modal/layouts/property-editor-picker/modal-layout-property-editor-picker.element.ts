import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../context';

import type { Subscription } from 'rxjs';
import type { UmbModalHandler } from '../../modal-handler';
import type { PropertyEditor } from '../../../../../mocks/data/property-editor.data';
import { UmbPropertyEditorStore } from '../../../../stores/property-editor.store';

export interface UmbModalPropertyEditorPickerData {
	selection?: Array<string>;
}

@customElement('umb-modal-layout-property-editor-picker')
export class UmbModalLayoutPropertyEditorPickerElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@property({ attribute: false })
	modalHandler?: UmbModalHandler;

	@property({ type: Object })
	data?: UmbModalPropertyEditorPickerData;

	@state()
	private _propertyEditors: Array<PropertyEditor> = [];

	@state()
	private _selection?: Array<string>;

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
		this._selection = this.data?.selection || [];
	}

	private _observePropertyEditors() {
		this._propertyEditorsSubscription = this._propertyEditorStore?.getAll().subscribe((propertyEditors) => {
			this._propertyEditors = propertyEditors;
		});
	}

	private _close() {
		this.modalHandler?.close({ selection: this._selection });
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
				<uui-box>${this._propertyEditors.map((propertyEditor) => html`<div>${propertyEditor.name}</div>`)}</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this._submit}></uui-button>
				</div>
			</umb-editor-entity-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-property-editor-picker': UmbModalLayoutPropertyEditorPickerElement;
	}
}
