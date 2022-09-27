import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription, distinctUntilChanged } from 'rxjs';

import { UmbContextConsumerMixin } from '../../../../../core/context';
import { UmbDataTypeContext } from '../../data-type.context';

import type { DataTypeDetails } from '../../../../../mocks/data/data-type.data';

@customElement('umb-editor-view-data-type-info')
export class UmbEditorViewDataTypeInfoElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@state()
	_dataType?: DataTypeDetails;

	private _dataTypeContext?: UmbDataTypeContext;
	private _dataTypeSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbDataTypeContext', (dataTypeContext) => {
			this._dataTypeContext = dataTypeContext;
			this._useDataType();
		});
	}

	private _useDataType() {
		this._dataTypeSubscription?.unsubscribe();

		this._dataTypeSubscription = this._dataTypeContext?.data
			.pipe(distinctUntilChanged())
			.subscribe((dataType: DataTypeDetails) => {
				this._dataType = dataType;
			});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._dataTypeSubscription?.unsubscribe();
	}

	render() {
		return html` ${this._renderGeneralInfo()}${this._renderReferences()} `;
	}

	private _renderGeneralInfo() {
		return html`
			<uui-box headline="General" style="margin-bottom: 20px;">
				<umb-editor-property-layout label="Key">
					<div slot="editor">${this._dataType?.key}</div>
				</umb-editor-property-layout>
				<umb-editor-property-layout
					label="Property Editor Alias
">
					<div slot="editor">${this._dataType?.propertyEditorAlias}</div>
				</umb-editor-property-layout>

				<umb-editor-property-layout
					label="Property Editor UI Alias
">
					<div slot="editor">${this._dataType?.propertyEditorUIAlias}</div>
				</umb-editor-property-layout>
			</uui-box>
		`;
	}

	private _renderReferences() {
		return html` <uui-box headline="References"> </uui-box> `;
	}
}

export default UmbEditorViewDataTypeInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-data-type-info': UmbEditorViewDataTypeInfoElement;
	}
}
