import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../core/context';
import type { DataTypeEntity } from '../../../../mocks/data/data-type.data';
import { Subscription, distinctUntilChanged } from 'rxjs';
import { UmbDataTypeContext } from '../data-type.context';
import { UmbModalService } from '../../../../core/services/modal';

@customElement('umb-editor-view-data-type-edit')
export class UmbEditorViewDataTypeEditElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@state()
	_dataType?: DataTypeEntity;

	private _dataTypeContext?: UmbDataTypeContext;

	private _dataTypeSubscription?: Subscription;

	private _modalService?: UmbModalService;

	constructor() {
		super();

		this.consumeContext('umbDataTypeContext', (dataTypeContext) => {
			this._dataTypeContext = dataTypeContext;
			this._useDataType();
		});

		this.consumeContext('umbModalService', (modalService) => {
			this._modalService = modalService;
		});
	}

	private _useDataType() {
		this._dataTypeSubscription?.unsubscribe();

		this._dataTypeSubscription = this._dataTypeContext?.data
			.pipe(distinctUntilChanged())
			.subscribe((dataType: DataTypeEntity) => {
				this._dataType = dataType;
			});
	}

	private _openPropertyEditorUIPicker() {
		if (!this._dataType) return;

		const selection = [this._dataType.propertyEditorUIAlias] || [];
		const modalHandler = this._modalService?.propertyEditorUIPicker({ selection });

		modalHandler?.onClose.then((returnValue) => {
			if (!this._dataType || !returnValue.selection) return;

			const propertyEditorUIAlias = returnValue.selection[0];
			this._dataType.propertyEditorUIAlias = propertyEditorUIAlias;
			this._dataTypeContext?.update({ propertyEditorUIAlias });
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._dataTypeSubscription?.unsubscribe();
	}

	render() {
		return html`
			<uui-box>
				<!-- TODO: temp property editor ui selector. Change when we have dialogs -->
				<h3>Property Editor UI</h3>
				${this._dataType?.propertyEditorUIAlias}
				<uui-button label="Change" @click=${this._openPropertyEditorUIPicker}></uui-button>
			</uui-box>
		`;
	}
}

export default UmbEditorViewDataTypeEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-data-type-edit': UmbEditorViewDataTypeEditElement;
	}
}
