import { UMB_MEDIA_TYPE_WORKSPACE_CONTEXT } from '../../media-type-workspace.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_PROPERTY_EDITOR_UI_PICKER_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { MediaTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbWorkspaceEditorViewExtensionElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-media-type-list-view-workspace-view')
export class UmbMediaTypeListViewWorkspaceViewEditElement
	extends UmbLitElement
	implements UmbWorkspaceEditorViewExtensionElement
{
	@state()
	_mediaType?: MediaTypeResponseModel;

	private _workspaceContext?: typeof UMB_MEDIA_TYPE_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_TYPE_WORKSPACE_CONTEXT, (_instance) => {
			this._workspaceContext = _instance;
			this._observeMediaType();
		});
	}

	private _observeMediaType() {
		if (!this._workspaceContext) {
			return;
		}

		this.observe(this._workspaceContext.data, (mediaType) => {
			this._mediaType = mediaType;
		});
	}

	render() {
		return html`<uui-box> List View view for ${this._mediaType?.alias}</uui-box>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
				padding-bottom: var(--uui-size-layout-1);
			}

			uui-box {
				margin-top: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbMediaTypeListViewWorkspaceViewEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-list-view-workspace-view': UmbMediaTypeListViewWorkspaceViewEditElement;
	}
}
