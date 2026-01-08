import '../../../block-type/components/input-block-type/index.js';
import { UMB_BLOCK_LIST_TYPE } from '../../constants.js';
import type { UmbBlockTypeBaseModel, UmbInputBlockTypeElement } from '@umbraco-cms/backoffice/block-type';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

/**
 * @element umb-property-editor-ui-block-list-type-configuration
 */
@customElement('umb-property-editor-ui-block-list-type-configuration')
export class UmbPropertyEditorUIBlockListBlockConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	#blockTypeWorkspaceModalRegistration?: UmbModalRouteRegistrationController<
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
			.addAdditionalPath(UMB_BLOCK_LIST_TYPE)
			.onSetup(() => {
				return { data: { entityType: UMB_BLOCK_LIST_TYPE, preset: {} }, modal: { size: 'large' } };
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
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<umb-input-block-type
			.value=${this.value}
			.workspacePath=${this._workspacePath}
			@create=${this.#onCreate}
			@delete=${this.#onChange}
			@change=${this.#onChange}></umb-input-block-type>`;
	}
}

export default UmbPropertyEditorUIBlockListBlockConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-list-type-configuration': UmbPropertyEditorUIBlockListBlockConfigurationElement;
	}
}
