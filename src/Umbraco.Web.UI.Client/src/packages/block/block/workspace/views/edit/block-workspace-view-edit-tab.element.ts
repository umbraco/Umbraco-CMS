import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../block-workspace.context-token.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbContentTypeModel, UmbPropertyTypeContainerMergedModel } from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './block-workspace-view-edit-properties.element.js';
// eslint-disable-next-line import-x/order
import type { UmbBlockWorkspaceElementManagerNames } from '../../block-workspace.context.js';

@customElement('umb-block-workspace-view-edit-tab')
export class UmbBlockWorkspaceViewEditTabElement extends UmbLitElement {
	@property({ attribute: false })
	public get managerName(): UmbBlockWorkspaceElementManagerNames | undefined {
		return this.#managerName;
	}
	public set managerName(value: UmbBlockWorkspaceElementManagerNames | undefined) {
		this.#managerName = value;
		this.#setStructureManager();
	}
	#managerName?: UmbBlockWorkspaceElementManagerNames;
	#blockWorkspace?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
	#groupStructureHelper = new UmbContentTypeContainerStructureHelper<UmbContentTypeModel>(this);

	@property({ type: String })
	public get containerId(): string | null | undefined {
		return this._containerId;
	}
	public set containerId(value: string | null | undefined) {
		this._containerId = value;
		this.#groupStructureHelper.setContainerId(value);
	}
	@state()
	private _containerId?: string | null;

	/**
	 * This is used by Inline Editing Mode of Block Editors, to simplify the visuals when possible.
	 */
	@property({ type: Boolean, reflect: false })
	hideSingleGroup = false;

	@state()
	private _groups?: Array<UmbPropertyTypeContainerMergedModel>;

	@state()
	private _hasProperties?: boolean;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#blockWorkspace = workspaceContext;
			this.#setStructureManager();
		});
	}

	#setStructureManager() {
		if (!this.#blockWorkspace || !this.#managerName) return;
		this.#groupStructureHelper.setStructureManager(this.#blockWorkspace[this.#managerName].structure);
		this.observe(
			this.#groupStructureHelper.childContainers,
			(groups) => {
				this._groups = groups;
			},
			'observeGroups',
		);
		this.observe(
			this.#groupStructureHelper.hasProperties,
			(hasProperties) => {
				this._hasProperties = hasProperties;
			},
			'observeHasProperties',
		);
	}

	override render() {
		if (this._containerId === undefined) return;
		return html`${this.#renderRootProperties()}${this.#renderGroups()}`;
	}

	#renderRootProperties() {
		// Only render the root properties if we have loaded both root properties and groups.
		if (!this._hasProperties || this._groups === undefined) return;
		if (this.hideSingleGroup && (!this._groups || this._groups.length === 0)) {
			return html`<umb-block-workspace-view-edit-properties
				.managerName=${this.#managerName}
				data-mark="property-group:root"
				.containerId=${this._containerId}></umb-block-workspace-view-edit-properties>`;
		}
		return html`<uui-box>
			<umb-block-workspace-view-edit-properties
				.managerName=${this.#managerName}
				data-mark="property-group:root"
				.containerId=${this._containerId}></umb-block-workspace-view-edit-properties>
		</uui-box>`;
	}

	#renderGroups() {
		if (!this._groups || this._groups.length === 0) return;
		if (this.hideSingleGroup && this._hasProperties === false && this._groups?.length === 1) {
			return this.renderGroup(this._groups[0]);
		}
		return repeat(
			this._groups,
			(group) => group.key,
			(group) => html`<uui-box .headline=${this.localize.string(group.name)}>${this.renderGroup(group)}</uui-box>`,
		);
	}

	renderGroup(group: UmbPropertyTypeContainerMergedModel) {
		return html`
			<umb-block-workspace-view-edit-properties
				.managerName=${this.#managerName}
				data-mark="property-group:${group.name}"
				.containerId=${group.ids[0]}></umb-block-workspace-view-edit-properties>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-box {
				--uui-box-default-padding: 0 var(--uui-size-space-5);
			}
			uui-box:not(:first-child) {
				margin-top: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbBlockWorkspaceViewEditTabElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-workspace-view-edit-tab': UmbBlockWorkspaceViewEditTabElement;
	}
}
