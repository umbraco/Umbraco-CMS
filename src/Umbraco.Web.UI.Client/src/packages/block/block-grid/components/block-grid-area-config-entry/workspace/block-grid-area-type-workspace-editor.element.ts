import { UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_CONTEXT } from './block-grid-area-type-workspace.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, css, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-block-grid-area-type-workspace-editor')
export class UmbBlockGridAreaTypeWorkspaceEditorElement extends UmbLitElement {
	#workspaceContext?: typeof UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_CONTEXT.TYPE;

	@state()
	_name?: string;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(this.#workspaceContext.name, (name) => {
				this._name = name;
			});
			this.#workspaceContext?.createPropertyDatasetContext(this);
		});
	}

	// TODO: Localization, make it so that the headline is about area configuration?
	override render() {
		return html`
			<umb-workspace-editor headline=${this.localize.term('blockEditor_blockConfigurationOverlayTitle', [this._name])}>
			</umb-workspace-editor>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export default UmbBlockGridAreaTypeWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-area-type-workspace-editor': UmbBlockGridAreaTypeWorkspaceEditorElement;
	}
}
