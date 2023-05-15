import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import {
	UmbModalRouteRegistrationController,
	UMB_PROPERTY_EDITOR_UI_PICKER_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extensions-registry';

// Note: Does only support picking a single data type. But this could be developed later into this same component. To follow other picker input components.
/**
 * Form control for picking a data type.
 * @element umb-input-data-type
 * @fires change - when the value of the input changes
 * @fires blur - when the input loses focus
 * @fires focus - when the input gains focus
 */
@customElement('umb-input-data-type')
export class UmbInputDataTypeElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	@state() private _selectedPropertyEditorUI?: ManifestPropertyEditorUI;

	/**
	 * @param {string} dataTypeId
	 * @default []
	 */
	@property({ attribute: false })
	set value(dataTypeId: string) {
		super.value = dataTypeId;
		this.#observeDataTypeId();
	}

	@state()
	private _modalRoute?: string;

	constructor() {
		super();

		console.log('make registration.');

		new UmbModalRouteRegistrationController(this, UMB_PROPERTY_EDITOR_UI_PICKER_MODAL)
			.onSetup(() => {
				return {
					selection: [this._value.toString()],
					submitLabel: 'Submit',
				};
			})
			.onSubmit((submitData) => {
				// TODO: we might should set the alias to null or empty string, if no selection.
				this.value = submitData.selection[0] ?? null;
				this.dispatchEvent(new CustomEvent('change', { composed: true, bubbles: true }));
			})
			.observeRouteBuilder((routeBuilder) => {
				this._modalRoute = routeBuilder(null);
				console.log('_modalRoute', this._modalRoute);
				this.requestUpdate('_modalRoute');
			});
	}

	#observeDataTypeId() {
		if (!this._value) return;

		this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('propertyEditorUI', this._value.toString()),
			(propertyEditorUI) => {
				if (!propertyEditorUI) return;

				this._selectedPropertyEditorUI = propertyEditorUI;
			},
			'observePropertyEditorUI'
		);
	}

	render() {
		return this._selectedPropertyEditorUI
			? html`
					<umb-ref-property-editor-ui
						name=${this._selectedPropertyEditorUI.meta.label}
						alias=${this._selectedPropertyEditorUI.alias}
						property-editor-model-alias=${this._selectedPropertyEditorUI.meta.propertyEditorModel}
						@open=${() => {
							console.log('TO BE DONE..');
						}}
						border>
						<uui-icon name="${this._selectedPropertyEditorUI.meta.icon}" slot="icon"></uui-icon>
						<uui-action-bar slot="actions">
							<uui-button label="Change" .href=${this._modalRoute}></uui-button>
						</uui-action-bar>
					</umb-ref-property-editor-ui>
			  `
			: html`
					<uui-button
						id="empty-state-button"
						label="Select Property Editor"
						look="placeholder"
						color="default"
						.href=${this._modalRoute}></uui-button>
			  `;
	}

	static styles = [
		UUITextStyles,
		css`
			#empty-state-button {
				width: 100%;
				--uui-button-padding-top-factor: 4;
				--uui-button-padding-bottom-factor: 4;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-data-type': UmbInputDataTypeElement;
	}
}
