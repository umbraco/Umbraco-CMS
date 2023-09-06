import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import {
	UmbModalRouteRegistrationController,
	UMB_DATA_TYPE_PICKER_FLOW_MODAL,
	UMB_WORKSPACE_MODAL,
} from '@umbraco-cms/backoffice/modal';

// Note: Does only support picking a single data type. But this could be developed later into this same component. To follow other picker input components.
/**
 * Form control for picking or creating a data type.
 * @element umb-data-type-flow-input
 * @fires change - when the value of the input changes
 * @fires blur - when the input loses focus
 * @fires focus - when the input gains focus
 */
@customElement('umb-data-type-flow-input')
export class UmbInputDataTypeElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	@state()
	private _ids?: Array<string>;

	/**
	 * @param {string} dataTypeId
	 * @default []
	 */
	@property({ attribute: false })
	get value(): string {
		return super.value?.toString() ?? '';
	}
	set value(dataTypeId: string) {
		super.value = dataTypeId ?? '';
		this._ids = super.value
			.split(',')
			.map((tag) => tag.trim())
			.filter((id) => id.length !== 0);
		console.log(this._ids);
	}

	#editDataTypeModal?: UmbModalRouteRegistrationController;

	@state()
	private _createRoute?: string;

	constructor() {
		super();

		this.#editDataTypeModal = new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL).onSetup(() => {
			return { entityType: 'data-type', preset: {} };
		});

		new UmbModalRouteRegistrationController(this, UMB_DATA_TYPE_PICKER_FLOW_MODAL)
			.onSetup(() => {
				return {
					selection: this._ids,
					submitLabel: 'Submit',
				};
			})
			.onSubmit((submitData) => {
				// TODO: we might should set the alias to null or empty string, if no selection.
				this.value = submitData.selection.join(',');
				this.dispatchEvent(new CustomEvent('change', { composed: true, bubbles: true }));
			})
			.observeRouteBuilder((routeBuilder) => {
				this._createRoute = routeBuilder(null);
			});
	}

	render() {
		return this._ids && this._ids.length > 0
			? html`
					<umb-ref-data-type
						data-type-id=${this._ids[0]}
						@open=${() => {
							// TODO: Could use something smarter for workspace modals, as I would like to avoid setting the rest of the URL here:
							this.#editDataTypeModal?.open({}, 'edit/' + this._ids![0]);
						}}
						border>
						<!-- TODO: Get the icon from property editor UI -->
						<uui-icon name="${'document'}" slot="icon"></uui-icon>
						<uui-action-bar slot="actions">
							<uui-button label="Change" .href=${this._createRoute}></uui-button>
						</uui-action-bar>
					</umb-ref-data-type>
			  `
			: html`
					<uui-button
						id="empty-state-button"
						label="Select Property Editor"
						look="placeholder"
						color="default"
						.href=${this._createRoute}></uui-button>
			  `;
	}

	static styles = [
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
		'umb-data-type-flow-input': UmbInputDataTypeElement;
	}
}
