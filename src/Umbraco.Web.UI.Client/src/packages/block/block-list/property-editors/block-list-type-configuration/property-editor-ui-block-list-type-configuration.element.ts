import type { UmbBlockTypeBaseModel, UmbInputBlockTypeElement } from '../../../block-type/index.js';
import '../../../block-type/components/input-block-type/index.js';
import { UMB_BLOCK_LIST_TYPE } from '../../types.js';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_WORKSPACE_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';

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

	@property({ attribute: false })
	value: UmbBlockTypeBaseModel[] = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#onCreate(e: CustomEvent) {
		const selectedElementType = e.detail.contentElementTypeKey;
		if (selectedElementType) {
			// We do not have any groups here, which is why we pass null as the groupKey.
			this.#blockTypeWorkspaceModalRegistration?.open({}, 'create/null/' + selectedElementType);
		}
	}

	render() {
		return html`<umb-input-block-type
			.value=${this.value}
			.workspacePath=${this._workspacePath}
			@create=${this.#onCreate}
			@change=${(e: Event) => {
				this.value = (e.target as UmbInputBlockTypeElement).value;
			}}></umb-input-block-type>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIBlockListBlockConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-list-type-configuration': UmbPropertyEditorUIBlockListBlockConfigurationElement;
	}
}
