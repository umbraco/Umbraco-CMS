import { html, customElement, css, ifDefined, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-folder-workspace-editor')
export class UmbFolderWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _entityType?: string;

	@state()
	private _isLoading = false;

	@state()
	private _isForbidden = false;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, (context) => {
			this.observe(context?.entityType, (entityType) => (this._entityType = entityType));
			this.observe(context?.loading.isOn, (isLoading) => (this._isLoading = isLoading ?? false));
			this.observe(context?.forbidden.isOn, (isForbidden) => (this._isForbidden = isForbidden ?? false));
		});
	}

	override render() {
		// Replaces the editor rather than disabling it: without folder data its header, and any
		// collection or action hosted in it, would otherwise still render.
		if (!this._isLoading && this._isForbidden) {
			return html`<umb-entity-detail-forbidden
				entity-type=${ifDefined(this._entityType)}></umb-entity-detail-forbidden>`;
		}

		return html`<umb-workspace-editor>
			<umb-icon id="icon" slot="header" name="icon-folder"></umb-icon>
			<umb-workspace-header-name-editable slot="header"></umb-workspace-header-name-editable>
		</umb-workspace-editor>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#icon {
				display: inline-block;
				font-size: var(--uui-size-6);
				margin-right: var(--uui-size-space-4);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-folder-workspace-editor': UmbFolderWorkspaceEditorElement;
	}
}
