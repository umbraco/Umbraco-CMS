import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { when } from 'lit-html/directives/when.js';
import { Subscription, map } from 'rxjs';
import { UmbModalService } from '../../../../../core/services/modal';

import { UmbContextConsumerMixin } from '../../../../../core/context';
import { UmbDataTypeContext } from '../../data-type.context';

import type { DataTypeEntity } from '../../../../../mocks/data/data-type.data';
import type { UmbExtensionRegistry } from '../../../../../core/extension';
import type { UmbPropertyEditorStore } from '../../../../../core/stores/property-editor.store';
import type { ManifestPropertyEditorUI } from '../../../../../core/models';
@customElement('umb-editor-view-data-type-edit')
export class UmbEditorViewDataTypeEditElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@state()
	_dataType?: DataTypeEntity;

	@state()
	private _propertyEditorIcon = '';

	@state()
	private _propertyEditorName = '';

	@state()
	private _propertyEditorAlias = '';

	@state()
	private _propertyEditorUIIcon = '';

	@state()
	private _propertyEditorUIName = '';

	@state()
	private _propertyEditorUIAlias = '';

	@state()
	private _availablePropertyEditorUIsCount = 0;

	private _dataTypeContext?: UmbDataTypeContext;
	private _extensionRegistry?: UmbExtensionRegistry;
	private _propertyEditorStore?: UmbPropertyEditorStore;

	private _dataTypeSubscription?: Subscription;
	private _propertyEditorSubscription?: Subscription;
	private _propertyEditorUISubscription?: Subscription;
	private _availablePropertyEditorUIsSubscription?: Subscription;

	private _modalService?: UmbModalService;

	constructor() {
		super();

		this.consumeContext('umbModalService', (modalService) => {
			this._modalService = modalService;
		});

		// TODO: wait for more contexts
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

		this._dataTypeSubscription = this._dataTypeContext?.data.subscribe((dataType: DataTypeEntity) => {
			this._dataType = dataType;

			if (!this._dataType) return;

			if (this._dataType.propertyEditorAlias !== this._propertyEditorAlias) {
				this._observePropertyEditor(this._dataType.propertyEditorAlias);
				this._observeAvailablePropertyEditorUIs(this._dataType.propertyEditorAlias);
			}

			if (this._dataType.propertyEditorUIAlias !== this._propertyEditorUIAlias) {
				this._observePropertyEditorUI(this._dataType.propertyEditorUIAlias);
			}
		});
	}

	private _observePropertyEditor(propertyEditorAlias: string) {
		if (!propertyEditorAlias) return;

		this._propertyEditorSubscription?.unsubscribe();

		this._propertyEditorSubscription = this._propertyEditorStore
			?.getByAlias(propertyEditorAlias)
			.subscribe((propertyEditor) => {
				this._propertyEditorName = propertyEditor?.name ?? '';
				this._propertyEditorAlias = propertyEditor?.alias ?? '';
				this._propertyEditorIcon = propertyEditor?.icon ?? '';
			});
	}

	private _observeAvailablePropertyEditorUIs(propertyEditorAlias: string) {
		if (!propertyEditorAlias) return;

		this._availablePropertyEditorUIsSubscription?.unsubscribe();

		this._availablePropertyEditorUIsSubscription = this._extensionRegistry
			?.extensionsOfType('propertyEditorUI')
			.pipe(
				map((propertyEditorUIs) =>
					propertyEditorUIs.filter((propertyEditorUI) =>
						propertyEditorUI?.meta?.propertyEditors?.includes(propertyEditorAlias)
					)
				)
			)
			.subscribe((availablePropertyEditorUIs) => {
				// Select the Property Editor UI if there is only one available
				this._availablePropertyEditorUIsCount = availablePropertyEditorUIs.length;
				if (availablePropertyEditorUIs?.length === 1) {
					this._selectPropertyEditorUI(availablePropertyEditorUIs[0].alias);
				}
			});
	}

	private _observePropertyEditorUI(propertyEditorUIAlias: string) {
		if (!propertyEditorUIAlias) return;

		this._propertyEditorUISubscription?.unsubscribe();

		this._propertyEditorSubscription = this._extensionRegistry
			?.getByAlias<ManifestPropertyEditorUI>(propertyEditorUIAlias)
			.subscribe((propertyEditorUI) => {
				this._propertyEditorUIName = propertyEditorUI?.name ?? '';
				this._propertyEditorUIAlias = propertyEditorUI?.alias ?? '';
				this._propertyEditorUIIcon = propertyEditorUI?.meta?.icon ?? '';
			});
	}

	private _openPropertyEditorPicker() {
		if (!this._dataType) return;

		const selection = [this._dataType.propertyEditorAlias] || [];
		const modalHandler = this._modalService?.propertyEditorPicker({ selection });

		modalHandler?.onClose().then(({ selection } = {}) => {
			if (!selection) return;

			const propertyEditorAlias = selection[0];
			this._selectPropertyEditor(propertyEditorAlias);
		});
	}

	private _selectPropertyEditor(propertyEditorAlias: string) {
		if (!this._dataType || this._dataType.propertyEditorUIAlias === propertyEditorAlias) return;

		this._dataType.propertyEditorAlias = propertyEditorAlias;
		this._dataTypeContext?.update({ propertyEditorAlias });
	}

	private _openPropertyEditorUIPicker() {
		if (!this._dataType) return;

		const selection = [this._dataType.propertyEditorUIAlias] || [];
		const modalHandler = this._modalService?.propertyEditorUIPicker({ selection });

		modalHandler?.onClose().then(({ selection } = {}) => {
			if (!selection) return;

			const propertyEditorUIAlias = selection[0];
			this._selectPropertyEditorUI(propertyEditorUIAlias);
		});
	}

	private _selectPropertyEditorUI(propertyEditorUIAlias: string) {
		if (!this._dataType || this._dataType.propertyEditorUIAlias === propertyEditorUIAlias) return;

		this._dataType.propertyEditorUIAlias = propertyEditorUIAlias;
		this._dataTypeContext?.update({ propertyEditorUIAlias });
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._dataTypeSubscription?.unsubscribe();
		this._propertyEditorSubscription?.unsubscribe();
		this._propertyEditorUISubscription?.unsubscribe();
		this._availablePropertyEditorUIsSubscription?.unsubscribe();
	}

	render() {
		return html`
			<uui-box>
				${this._renderPropertyEditor()}
				${when(this._dataType?.propertyEditorAlias, () => html` ${this._renderPropertyEditorUI()} `)}</uui-box
			>
		`;
	}

	private _renderPropertyEditor() {
		return html`
			<h3>Property Editor</h3>

			${this._dataType?.propertyEditorAlias
				? html`
						<!-- TODO: border is a bit weird attribute name. Maybe single or standalone would be better? -->
						<umb-ref-property-editor .name=${this._propertyEditorName} .alias=${this._propertyEditorAlias} border>
							<uui-icon name="${this._propertyEditorIcon}" slot="icon"></uui-icon>
							<uui-action-bar slot="actions">
								<uui-button label="Change" @click=${this._openPropertyEditorPicker}></uui-button>
							</uui-action-bar>
						</umb-ref-property-editor>
				  `
				: html`<uui-button
						label="Select Property Editor"
						look="placeholder"
						color="default"
						@click=${this._openPropertyEditorPicker}></uui-button>`}
		`;
	}

	private _renderPropertyEditorUI() {
		return html`
			<h3>Property Editor UI</h3>

			${this._dataType?.propertyEditorUIAlias
				? html`
						<!-- TODO: border is a bit weird attribute name. Maybe single or standalone would be better? -->
						<umb-ref-property-editor-ui
							.name=${this._propertyEditorUIName}
							.alias=${this._propertyEditorUIAlias}
							border>
							<uui-icon name="${this._propertyEditorUIIcon}" slot="icon"></uui-icon>

							${this._availablePropertyEditorUIsCount > 1
								? html`
										<uui-action-bar slot="actions">
											<uui-button label="Change" @click=${this._openPropertyEditorUIPicker}></uui-button>
										</uui-action-bar>
								  `
								: nothing}
						</umb-ref-property-editor-ui>
				  `
				: html`
						${this._availablePropertyEditorUIsCount > 0
							? html`<uui-button
									label="Select Property Editor UI"
									look="placeholder"
									color="default"
									@click=${this._openPropertyEditorUIPicker}></uui-button>`
							: html`No Property Editor UIs registered for this Property Editor`}
				  `}
		`;
	}
}

export default UmbEditorViewDataTypeEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-data-type-edit': UmbEditorViewDataTypeEditElement;
	}
}
