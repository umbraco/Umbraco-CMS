import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../block-workspace.context-token.js';
import type { UmbBlockWorkspaceElementManagerNames } from '../../block-workspace.context.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbPropertyContainerTypes,
	UmbContentTypeModel,
	UmbPropertyTypeModel,
} from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypePropertyStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-block-workspace-view-edit-properties')
export class UmbBlockWorkspaceViewEditPropertiesElement extends UmbLitElement {
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
	#propertyStructureHelper = new UmbContentTypePropertyStructureHelper<UmbContentTypeModel>(this);

	@property({ type: String, attribute: 'container-name', reflect: false })
	public get containerName(): string | undefined {
		return this.#propertyStructureHelper.getContainerName();
	}
	public set containerName(value: string | undefined) {
		this.#propertyStructureHelper.setContainerName(value);
	}

	@property({ type: String, attribute: 'container-type', reflect: false })
	public get containerType(): UmbPropertyContainerTypes | undefined {
		return this.#propertyStructureHelper.getContainerType();
	}
	public set containerType(value: UmbPropertyContainerTypes | undefined) {
		this.#propertyStructureHelper.setContainerType(value);
	}

	@state()
	_propertyStructure: Array<UmbPropertyTypeModel> = [];

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#blockWorkspace = workspaceContext;
			this.#setStructureManager();
		});
	}

	#setStructureManager() {
		if (!this.#blockWorkspace || !this.#managerName) return;
		this.#propertyStructureHelper.setStructureManager(this.#blockWorkspace[this.#managerName].structure);
		this.observe(
			this.#propertyStructureHelper.propertyStructure,
			(propertyStructure) => {
				this._propertyStructure = propertyStructure;
			},
			'observePropertyStructure',
		);
	}

	render() {
		return repeat(
			this._propertyStructure,
			(property) => property.alias,
			(property) => html`<umb-property-type-based-property .property=${property}></umb-property-type-based-property> `,
		);
	}

	static styles = [
		UmbTextStyles,
		css`
			umb-property-type-based-property {
				border-bottom: 1px solid var(--uui-color-divider);
			}
			umb-property-type-based-property:last-child {
				border-bottom: 0;
			}
		`,
	];
}

export default UmbBlockWorkspaceViewEditPropertiesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-workspace-view-edit-properties': UmbBlockWorkspaceViewEditPropertiesElement;
	}
}
