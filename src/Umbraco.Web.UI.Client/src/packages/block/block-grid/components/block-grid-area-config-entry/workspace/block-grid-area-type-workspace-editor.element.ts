import { UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_CONTEXT } from './block-grid-area-type-workspace.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, css, html, state, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-block-grid-area-type-workspace-editor')
export class UmbBlockGridAreaTypeWorkspaceEditorElement extends UmbLitElement {
	//
	#workspaceContext?: typeof UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_CONTEXT.TYPE;

	@state()
	_name?: string;

	@property({ type: String, attribute: false })
	workspaceAlias?: string;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#workspaceContext?.createPropertyDatasetContext(this);
		});
	}

	// TODO: Localization, make it so that the headline is about area configuration?
	render() {
		return this.workspaceAlias
			? html`
					<umb-workspace-editor
						alias=${this.workspaceAlias}
						headline=${this.localize.term('blockEditor_blockConfigurationOverlayTitle', [this._name])}>
					</umb-workspace-editor>
			  `
			: '';
	}

	static styles = [
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
