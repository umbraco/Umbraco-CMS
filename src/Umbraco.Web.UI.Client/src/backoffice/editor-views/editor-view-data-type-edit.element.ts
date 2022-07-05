import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbContextConsumerMixin } from '../../core/context';
import type { DataTypeEntity } from '../../mocks/data/content.data';
import type { UmbExtensionManifestPropertyEditorUI, UmbExtensionRegistry } from '../../core/extension';

@customElement('umb-editor-view-data-type-edit')
export class UmbEditorViewDataTypeEditElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@property({ type: Object })
	dataType?: DataTypeEntity;

	@state()
	private _propertyEditorUIs: Array<UmbExtensionManifestPropertyEditorUI> = [];

	private _extensionRegistry?: UmbExtensionRegistry;

	constructor() {
		super();

		this.consumeContext('umbExtensionRegistry', (registry) => {
			this._extensionRegistry = registry;
			this._usePropertyEditorUIs();
		});
	}

	private _usePropertyEditorUIs() {
		this._extensionRegistry?.extensionsOfType('propertyEditorUI').subscribe((propertyEditorUIs) => {
			this._propertyEditorUIs = propertyEditorUIs;
		});
	}

	render() {
		return html`
			<uui-box>
				<h3>Property Editor (Model/Schema?)</h3>
				Selector goes here

				<!-- TODO: temp property editor ui selector. Change when we have dialogs -->
				<h3>Property Editor UI</h3>
				<uui-combobox-list value="${ifDefined(this.dataType?.propertyEditorUIAlias)}">
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

				<hr style="margin-top: 20px;" />

				<h3>Config/Prevalues</h3>
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
