import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbModalService } from '../../../../../core/services/modal';

import { UmbContextConsumerMixin } from '../../../../../core/context';
import { UmbDataTypeContext } from '../../data-type.context';

import type { DataTypeDetails } from '../../../../../mocks/data/data-type.data';
import type { UmbExtensionRegistry } from '../../../../../core/extension';
import type { UmbPropertyEditorStore } from '../../../../../core/stores/property-editor/property-editor.store';
import type { ManifestPropertyEditorUI } from '../../../../../core/models';

import '../../shared/property-editor-config.element';

@customElement('umb-editor-view-data-type-edit')
export class UmbEditorViewDataTypeEditElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@state()
	_dataType?: DataTypeDetails;

	@state()
	private _propertyEditorUIIcon = '';

	@state()
	private _propertyEditorUIName = '';

	@state()
	private _propertyEditorUIAlias = '';

	@state()
	private _propertyEditorAlias = '';

	@state()
	private _data: Array<any> = [];

	private _dataTypeContext?: UmbDataTypeContext;
	private _extensionRegistry?: UmbExtensionRegistry;
	private _propertyEditorStore?: UmbPropertyEditorStore;

	private _dataTypeSubscription?: Subscription;
	private _propertyEditorSubscription?: Subscription;
	private _propertyEditorUISubscription?: Subscription;

	private _modalService?: UmbModalService;

	constructor() {
		super();

		// TODO: wait for more contexts
		this.consumeContext('umbModalService', (modalService) => {
			this._modalService = modalService;
		});

		this.consumeContext('umbDataTypeContext', (dataTypeContext) => {
			this._dataTypeContext = dataTypeContext;
			this._observeDataType();
		});

		this.consumeContext('umbPropertyEditorStore', (propertyEditorStore) => {
			this._propertyEditorStore = propertyEditorStore;
			this._observeDataType();
		});

		this.consumeContext('umbExtensionRegistry', (extensionRegistry) => {
			this._extensionRegistry = extensionRegistry;
			this._observeDataType();
		});
	}

	private _observeDataType() {
		if (!this._dataTypeContext || !this._propertyEditorStore || !this._extensionRegistry) return;

		this._dataTypeSubscription?.unsubscribe();

		this._dataTypeSubscription = this._dataTypeContext?.data.subscribe((dataType: DataTypeDetails) => {
			this._dataType = dataType;

			if (!this._dataType) return;

			if (this._dataType.propertyEditorAlias !== this._propertyEditorAlias) {
				this._observePropertyEditor(this._dataType.propertyEditorAlias);
			}

			if (this._dataType.propertyEditorUIAlias !== this._propertyEditorUIAlias) {
				this._observePropertyEditorUI(this._dataType.propertyEditorUIAlias);
			}

			if (this._dataType.data !== this._data) {
				this._data = this._dataType.data;
			}
		});
	}

	private _observePropertyEditorUI(propertyEditorUIAlias: string | null) {
		if (!propertyEditorUIAlias) return;

		this._propertyEditorUISubscription?.unsubscribe();

		this._propertyEditorUISubscription = this._extensionRegistry
			?.getByAlias<ManifestPropertyEditorUI>(propertyEditorUIAlias)
			.subscribe((propertyEditorUI) => {
				this._propertyEditorUIName = propertyEditorUI?.name ?? '';
				this._propertyEditorUIAlias = propertyEditorUI?.alias ?? '';
				this._propertyEditorUIIcon = propertyEditorUI?.meta?.icon ?? '';

				this._observePropertyEditor(propertyEditorUI?.meta?.propertyEditor ?? '');
			});
	}

	private _observePropertyEditor(propertyEditorAlias: string | null) {
		if (!propertyEditorAlias) return;

		this._propertyEditorSubscription?.unsubscribe();

		this._propertyEditorSubscription = this._propertyEditorStore
			?.getByAlias(propertyEditorAlias)
			.subscribe((propertyEditor) => {
				this._propertyEditorAlias = propertyEditor?.alias ?? '';
			});
	}

	private _openPropertyEditorUIPicker() {
		if (!this._dataType) return;

		const modalHandler = this._modalService?.propertyEditorUIPicker({
			selection: this._dataType.propertyEditorUIAlias ? [this._dataType.propertyEditorUIAlias] : [],
		});

		modalHandler?.onClose().then(({ selection } = {}) => {
			if (!selection) return;

			const propertyEditorUIAlias = selection[0];
			this._selectPropertyEditorUI(propertyEditorUIAlias);
		});
	}

	private _selectPropertyEditorUI(propertyEditorUIAlias: string | null) {
		if (!this._dataType || this._dataType.propertyEditorUIAlias === propertyEditorUIAlias) return;

		this._dataType.propertyEditorUIAlias = propertyEditorUIAlias;
		this._dataTypeContext?.update({ propertyEditorUIAlias });
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._dataTypeSubscription?.unsubscribe();
		this._propertyEditorSubscription?.unsubscribe();
		this._propertyEditorUISubscription?.unsubscribe();
	}

	render() {
		return html`
			<uui-box headline="Property Editor" style="margin-bottom: 20px;"> ${this._renderPropertyEditorUI()} </uui-box>
			${this._renderConfig()} </uui-box>
		`;
	}

	private _renderPropertyEditorUI() {
		return html`
			<h4>Property Editor UI</h4>

			${this._dataType?.propertyEditorUIAlias
				? html`
						<!-- TODO: border is a bit weird attribute name. Maybe single or standalone would be better? -->
						<umb-ref-property-editor-ui
							name=${this._propertyEditorUIName}
							alias=${this._propertyEditorUIAlias}
							property-editor-alias=${this._propertyEditorAlias}
							border>
							<uui-icon name="${this._propertyEditorUIIcon}" slot="icon"></uui-icon>
							<uui-action-bar slot="actions">
								<uui-button label="Change" @click=${this._openPropertyEditorUIPicker}></uui-button>
							</uui-action-bar>
						</umb-ref-property-editor-ui>
				  `
				: html`
						<uui-button
							label="Select Property Editor UI"
							look="placeholder"
							color="default"
							@click=${this._openPropertyEditorUIPicker}></uui-button>
				  `}
		`;
	}

	private _renderConfig() {
		return html`
			${this._propertyEditorAlias && this._propertyEditorUIAlias
				? html`
						<uui-box headline="Config">
							<umb-property-editor-config
								property-editor-alias="${this._propertyEditorAlias}"
								property-editor-ui-alias="${this._propertyEditorUIAlias}"
								.data="${this._data}"></umb-property-editor-config>
						</uui-box>
				  `
				: nothing}
		`;
	}
}

export default UmbEditorViewDataTypeEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-data-type-edit': UmbEditorViewDataTypeEditElement;
	}
}
