import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription, distinctUntilChanged } from 'rxjs';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbModalService } from '../../../../../core/services/modal';

import { UmbContextConsumerMixin } from '../../../../../core/context';
import type { ManifestPropertyEditorUI } from '../../../../../core/models';
import { UmbDataTypeContext } from '../../data-type.context';

import type { DataTypeEntity } from '../../../../../mocks/data/data-type.data';
import type { UmbExtensionRegistry } from '../../../../../core/extension';
@customElement('umb-editor-view-data-type-edit')
export class UmbEditorViewDataTypeEditElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@state()
	_dataType?: DataTypeEntity;

	@state()
	private _propertyEditorUIs: Array<ManifestPropertyEditorUI> = [];

	private _extensionRegistry?: UmbExtensionRegistry;
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
				<h3>Property Editor UI</h3>
				<!-- TODO: border is a bit weird attribute name. Maybe single or standalone would be better? -->
				<umb-ref-property-editor-ui alias="${ifDefined(this._dataType?.propertyEditorUIAlias)}" border>
					<uui-action-bar slot="actions">
						<uui-button label="Change" @click=${this._openPropertyEditorUIPicker}></uui-button>
					</uui-action-bar>
				</umb-ref-property-editor-ui>
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
