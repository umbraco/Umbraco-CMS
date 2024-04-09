import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../../media-workspace.context-token.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyTypeContainerModel } from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './media-workspace-view-edit-properties.element.js';
@customElement('umb-media-workspace-view-edit-tab')
export class UmbMediaWorkspaceViewEditTabElement extends UmbLitElement {
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
	_groups: Array<UmbPropertyTypeContainerModel> = [];

	@state()
	_hasProperties = false;

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (workspaceContext) => {
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
							<umb-media-workspace-view-edit-properties
								class="properties"
								.containerId=${this._containerId}></umb-media-workspace-view-edit-properties>
						</uui-box>
					`
				: ''}
			${repeat(
				this._groups,
				(group) => group.name,
				(group) =>
					html`<uui-box .headline=${group.name || ''}>
						<umb-media-workspace-view-edit-properties
							class="properties"
							.containerId=${group.id}></umb-media-workspace-view-edit-properties>
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

export default UmbMediaWorkspaceViewEditTabElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace-view-edit-tab': UmbMediaWorkspaceViewEditTabElement;
	}
}
