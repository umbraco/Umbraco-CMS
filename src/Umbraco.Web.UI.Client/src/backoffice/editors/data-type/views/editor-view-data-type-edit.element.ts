import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbContextConsumerMixin } from '../../../../core/context';
import type { DataTypeEntity } from '../../../../mocks/data/data-type.data';
import type { UmbExtensionManifestPropertyEditorUI, UmbExtensionRegistry } from '../../../../core/extension';
import { Subscription, distinctUntilChanged } from 'rxjs';
import { UmbDataTypeContext } from '../data-type.context';
import { UUIComboboxListElement, UUIComboboxListEvent } from '@umbraco-ui/uui';

@customElement('umb-editor-view-data-type-edit')
export class UmbEditorViewDataTypeEditElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@state()
	_dataType?: DataTypeEntity;

	@state()
	private _propertyEditorUIs: Array<UmbExtensionManifestPropertyEditorUI> = [];

	private _extensionRegistry?: UmbExtensionRegistry;
	private _dataTypeContext?: UmbDataTypeContext;

	private _propertyEditorUIsSubscription?: Subscription;
	private _dataTypeSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbExtensionRegistry', (registry) => {
			this._extensionRegistry = registry;
			this._usePropertyEditorUIs();
		});

		this.consumeContext('umbDataTypeContext', (dataTypeContext) => {
			this._dataTypeContext = dataTypeContext;
			this._useDataType();
		});
	}

	private _usePropertyEditorUIs() {
		this._propertyEditorUIsSubscription = this._extensionRegistry
			?.extensionsOfType('propertyEditorUI')
			.subscribe((propertyEditorUIs) => {
				this._propertyEditorUIs = propertyEditorUIs;
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

	private _handleChange(event: UUIComboboxListEvent) {
		if (!this._dataType) return;

		const target = event.composedPath()[0] as UUIComboboxListElement;
		const value = target.value as string;

		this._dataTypeContext?.update({ propertyEditorUIAlias: value });
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();

		this._propertyEditorUIsSubscription?.unsubscribe();
		this._dataTypeSubscription?.unsubscribe();
	}

	render() {
		return html`
			<uui-box>
				<!-- TODO: temp property editor ui selector. Change when we have dialogs -->
				<h3>Property Editor UI</h3>
				<uui-combobox-list value="${ifDefined(this._dataType?.propertyEditorUIAlias)}" @change="${this._handleChange}">
					${this._propertyEditorUIs.map(
						(propertyEditorUI) =>
							html`<uui-combobox-list-option style="padding: 8px; margin: 0;" value="${propertyEditorUI.alias}">
								<div style="display: flex; align-items: center;">
									<uui-icon style="margin-right: 5px;" name="${propertyEditorUI.meta.icon}"></uui-icon>
									${propertyEditorUI.name}
								</div>
							</uui-combobox-list-option>`
					)}
				</uui-combobox-list>
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
