import { UMB_MEDIA_TYPE_WORKSPACE_CONTEXT } from '../../media-type-workspace.context-token.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UUIBooleanInputEvent, UUIToggleElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-media-type-workspace-view-settings')
export class UmbMediaTypeWorkspaceViewSettingsElement extends UmbLitElement implements UmbWorkspaceViewElement {
	#workspaceContext?: typeof UMB_MEDIA_TYPE_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _variesByCulture?: boolean;

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_TYPE_WORKSPACE_CONTEXT, (mediaTypeContext) => {
			this.#workspaceContext = mediaTypeContext;
			this.#observeMediaType();
		});
	}

	#observeMediaType() {
		if (!this.#workspaceContext) return;
		this.observe(
			this.#workspaceContext.variesByCulture,
			(variesByCulture) => (this._variesByCulture = variesByCulture),
		);
	}

	#onChangeVariesByCulture(event: UUIBooleanInputEvent) {
		this.#workspaceContext?.setVariesByCulture((event.target as UUIToggleElement).checked);
	}

	override render() {
		return html`
			<uui-box headline="Data variations">
				<umb-property-layout
					alias="VaryByCulture"
					label=${this.localize.term('contentTypeEditor_cultureVariantHeading')}>
					<div slot="description">
						<umb-localize key="contentTypeEditor_cultureVariantDescription"
							>Allow editors to create content of different languages.</umb-localize
						>
					</div>
					<div slot="editor">
						<uui-toggle
							?checked=${this._variesByCulture}
							@change=${this.#onChangeVariesByCulture}
							label=${this.localize.term('contentTypeEditor_cultureVariantLabel')}></uui-toggle>
					</div>
				</umb-property-layout>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
				padding-bottom: var(--uui-size-layout-1); // To enforce some distance to the bottom of the scroll-container.
			}

			uui-toggle {
				display: flex;
			}
		`,
	];
}

export default UmbMediaTypeWorkspaceViewSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-workspace-view-settings': UmbMediaTypeWorkspaceViewSettingsElement;
	}
}
