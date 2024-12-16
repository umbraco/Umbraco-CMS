import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../block-workspace.context-token.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbContentTypeModel, UmbPropertyTypeContainerModel } from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './block-workspace-view-edit-properties.element.js';
// eslint-disable-next-line import/order
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
	 * If true, the group box will be hidden, if we are to only represents one group.
	 * This is used by Inline Editing Mode of Block List Editor.
	 */
	@property({ type: Boolean, reflect: false })
	hideSingleGroup = false;

	@state()
	_groups: Array<UmbPropertyTypeContainerModel> = [];

	@state()
	_hasProperties = false;

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
			this.#groupStructureHelper.mergedContainers,
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
		return html`
			${this._hasProperties
				? html` <umb-block-workspace-view-edit-properties
						.managerName=${this.#managerName}
						data-mark="property-group:root"
						.containerId=${this._containerId}></umb-block-workspace-view-edit-properties>`
				: ''}
			${this.hideSingleGroup && this._groups.length === 1
				? this.renderGroup(this._groups[0])
				: repeat(
						this._groups,
						(group) => group.id,
						(group) => html` <uui-box .headline=${group.name}>${this.renderGroup(group)}</uui-box>`,
					)}
		`;
	}

	renderGroup(group: UmbPropertyTypeContainerModel) {
		return html`
			<umb-block-workspace-view-edit-properties
				.managerName=${this.#managerName}
				data-mark="property-group:${group.name}"
				.containerId=${group.id}></umb-block-workspace-view-edit-properties>
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
