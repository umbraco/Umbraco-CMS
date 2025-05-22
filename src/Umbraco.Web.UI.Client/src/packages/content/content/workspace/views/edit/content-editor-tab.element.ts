import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbContentTypeModel,
	UmbContentTypeStructureManager,
	UmbPropertyTypeContainerModel,
} from '@umbraco-cms/backoffice/content-type';
import {
	UmbContentTypeContainerStructureHelper,
	UMB_PROPERTY_STRUCTURE_WORKSPACE_CONTEXT,
} from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import './content-editor-properties.element.js';

@customElement('umb-content-workspace-view-edit-tab')
export class UmbContentWorkspaceViewEditTabElement extends UmbLitElement {
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

	#groupStructureHelper = new UmbContentTypeContainerStructureHelper<UmbContentTypeModel>(this);

	@state()
	_groups: Array<UmbPropertyTypeContainerModel> = [];

	@state()
	_hasProperties = false;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_STRUCTURE_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#groupStructureHelper.setStructureManager(
				// Assuming its the same content model type that we are working with here... [NL]
				workspaceContext?.structure as unknown as UmbContentTypeStructureManager<UmbContentTypeModel>,
			);
		});
		this.observe(this.#groupStructureHelper.mergedContainers, (groups) => {
			this._groups = groups;
		});
		this.observe(this.#groupStructureHelper.hasProperties, (hasProperties) => {
			this._hasProperties = hasProperties;
		});
	}

	override render() {
		return html`
			${this._hasProperties
				? html`
						<uui-box>
							<umb-content-workspace-view-edit-properties
								data-mark="property-group:root"
								.containerId=${this._containerId}></umb-content-workspace-view-edit-properties>
						</uui-box>
					`
				: ''}
			${repeat(
				this._groups,
				(group) => group.id,
				(group) =>
					html`<uui-box .headline=${this.localize.string(group.name) ?? ''}>
						<umb-content-workspace-view-edit-properties
							data-mark="property-group:${group.name}"
							.containerId=${group.id}></umb-content-workspace-view-edit-properties>
					</uui-box>`,
			)}
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

export default UmbContentWorkspaceViewEditTabElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-workspace-view-edit-tab': UmbContentWorkspaceViewEditTabElement;
	}
}
