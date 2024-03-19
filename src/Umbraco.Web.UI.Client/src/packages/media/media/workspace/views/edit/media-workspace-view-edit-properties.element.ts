import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../../media-workspace.context-token.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypePropertyStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-media-workspace-view-edit-properties')
export class UmbMediaWorkspaceViewEditPropertiesElement extends UmbLitElement {
	@property({ type: String, attribute: 'container-name', reflect: false })
	public get containerId(): string | null | undefined {
		return this._propertyStructureHelper.getContainerId();
	}
	public set containerId(value: string | null | undefined) {
		this._propertyStructureHelper.setContainerId(value);
	}

	_propertyStructureHelper = new UmbContentTypePropertyStructureHelper<any>(this);

	@state()
	_propertyStructure: Array<UmbPropertyTypeModel> = [];

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._propertyStructureHelper.setStructureManager(workspaceContext.structure);
		});
		this.observe(this._propertyStructureHelper.propertyStructure, (propertyStructure) => {
			this._propertyStructure = propertyStructure;
		});
	}

	render() {
		return repeat(
			this._propertyStructure,
			(property) => property.alias,
			(property) =>
				html`<umb-property-type-based-property
					class="property"
					.property=${property}></umb-property-type-based-property> `,
		);
	}

	static styles = [
		UmbTextStyles,
		css`
			.property {
				border-bottom: 1px solid var(--uui-color-divider);
			}
			.property:last-child {
				border-bottom: 0;
			}
		`,
	];
}

export default UmbMediaWorkspaceViewEditPropertiesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace-view-edit-properties': UmbMediaWorkspaceViewEditPropertiesElement;
	}
}
