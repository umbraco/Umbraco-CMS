import { customElement, html, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbInputBlockTypeElement } from '@umbraco-cms/backoffice/block-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UMB_BLOCK_RTE_TYPE } from '@umbraco-cms/backoffice/block-rte';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';

/**
 * @element umb-property-editor-ui-block-rte-type-configuration
 */
@customElement('umb-property-editor-ui-block-rte-type-configuration')
export class UmbPropertyEditorUIBlockRteBlockConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	readonly #blockTypeWorkspaceModalRegistration?: UmbModalRouteRegistrationController<
		typeof UMB_WORKSPACE_MODAL.DATA,
		typeof UMB_WORKSPACE_MODAL.VALUE
	>;

	@property({ attribute: false })
	value: UmbBlockTypeBaseModel[] = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	@state()
	private _workspacePath?: string;

	constructor() {
		super();
		this.#blockTypeWorkspaceModalRegistration?.destroy();

		this.#blockTypeWorkspaceModalRegistration = new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath(UMB_BLOCK_RTE_TYPE)
			.onSetup(() => {
				return { data: { entityType: UMB_BLOCK_RTE_TYPE, preset: {} }, modal: { size: 'large' } };
			})
			.observeRouteBuilder((routeBuilder) => {
				const newpath = routeBuilder({});
				this._workspacePath = newpath;
			});
	}

	#onCreate(e: CustomEvent) {
		const selectedElementType = e.detail.contentElementTypeKey;
		if (selectedElementType) {
			this.#blockTypeWorkspaceModalRegistration?.open({}, 'create/' + selectedElementType + '/null');
		}
	}
	#onChange(e: CustomEvent) {
		e.stopPropagation();
		this.value = (e.target as UmbInputBlockTypeElement).value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return UmbInputBlockTypeElement
			? html`<umb-input-block-type
					.value=${this.value}
					.workspacePath=${this._workspacePath}
					@create=${this.#onCreate}
					@change=${this.#onChange}
					@delete=${this.#onChange}></umb-input-block-type>`
			: nothing;
	}
}

export default UmbPropertyEditorUIBlockRteBlockConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-rte-type-configuration': UmbPropertyEditorUIBlockRteBlockConfigurationElement;
	}
}
