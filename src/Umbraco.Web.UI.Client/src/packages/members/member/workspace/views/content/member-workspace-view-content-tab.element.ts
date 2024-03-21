import { UMB_MEMBER_WORKSPACE_CONTEXT } from '../../member-workspace.context-token.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { PropertyTypeContainerModelBaseModel } from '@umbraco-cms/backoffice/external/backend-api';

import './member-workspace-view-content-properties.element.js';
@customElement('umb-member-workspace-view-content-tab')
export class UmbMemberWorkspaceViewContentTabElement extends UmbLitElement {
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

	#groupStructureHelper = new UmbContentTypeContainerStructureHelper<any>(this);

	@state()
	_groups: Array<PropertyTypeContainerModelBaseModel> = [];

	@state()
	_hasProperties = false;

	constructor() {
		super();

		this.consumeContext(UMB_MEMBER_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#groupStructureHelper.setStructureManager(workspaceContext.structure);
		});
		this.observe(this.#groupStructureHelper.mergedContainers, (groups) => {
			this._groups = groups;
		});
		this.observe(this.#groupStructureHelper.hasProperties, (hasProperties) => {
			this._hasProperties = hasProperties;
		});
	}

	render() {
		return html`
			${this._hasProperties
				? html`
						<uui-box>
							<umb-member-workspace-view-content-properties
								class="properties"
								.containerId=${this._containerId}></umb-member-workspace-view-content-properties>
						</uui-box>
					`
				: ''}
			${repeat(
				this._groups,
				(group) => group.name,
				(group) =>
					html`<uui-box .headline=${group.name || ''}>
						<umb-member-workspace-view-content-properties
							class="properties"
							.containerId=${group.id}></umb-member-workspace-view-content-properties>
					</uui-box>`,
			)}
		`;
	}

	static styles = [
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

export default UmbMemberWorkspaceViewContentTabElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-workspace-view-content-tab': UmbMemberWorkspaceViewContentTabElement;
	}
}
