import { UmbBlockTypeBase } from '@umbraco-cms/backoffice/block';
import { UmbBlockTypeWorkspaceContext } from '../../../block-type/workspace/block-type-workspace.context.js';
import { css, html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-block-grid-type-workspace-view')
export class UmbBlockTypeGridWorkspaceViewSettingsElement extends UmbLitElement implements UmbWorkspaceViewElement {
	#labelOnTopSetting = { alias: 'labelOnTop', value: true };

	@state()
	private _showSizeOptions = false;

	@state()
	private _backgroundColor?: string;

	@state()
	private _contentElementTypeKey?: string;

	@state()
	private _editorSize?: string;

	@state()
	private _iconColor?: string;

	@state()
	private _label?: string;

	@state()
	private _settingsElementTypeKey?: string;

	@state()
	private _stylesheet?: string;

	@state()
	private _view?: string;

	constructor() {
		super();
		this.consumeContext(UMB_WORKSPACE_CONTEXT, (instance) => {
			const workspace = instance as UmbBlockTypeWorkspaceContext;
			this.observe(workspace.data, (data) => {
				this._backgroundColor = data?.backgroundColor;
				this._contentElementTypeKey = data?.contentElementTypeKey;
				this._editorSize = data?.editorSize;
				this._iconColor = data?.iconColor;
				this._label = data?.label;
				this._settingsElementTypeKey = data?.settingsElementTypeKey;
				this._stylesheet = data?.stylesheet;
				this._view = data?.view;
			});
		});
	}

	render() {
		return html`
			<uui-box headline=${this.localize.term('general_general')}>
				<umb-property
					label=${this.localize.term('general_label')}
					alias="label"
					.value=${this._label}
					property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"
					.config=${[this.#labelOnTopSetting]}></umb-property>
				<umb-property
					label=${this.localize.term('blockEditor_labelContentElementType')}
					read-only
					alias="contentElementTypeKey"
					.value=${[this._contentElementTypeKey]}
					property-editor-ui-alias="Umb.PropertyEditorUi.DocumentTypePicker"
					.config=${[
						{ alias: 'onlyPickElementTypes', value: true },
						{ alias: 'validationLimit', value: { min: 0, max: 1 } },
					]}></umb-property>
				<umb-property
					label=${this.localize.term('blockEditor_labelSettingsElementType')}
					alias="settingsElementTypeKey"
					.value=${[this._settingsElementTypeKey]}
					property-editor-ui-alias="Umb.PropertyEditorUi.DocumentTypePicker"
					.config=${[
						{ alias: 'onlyPickElementTypes', value: true },
						{ alias: 'validationLimit', value: { min: 0, max: 1 } },
					]}></umb-property>
			</uui-box>
			<uui-box headline=${this.localize.term('blockEditor_headlineAllowance')}>
				<umb-property
					label=${this.localize.term('blockEditor_allowBlockInRoot')}
					alias="allowInRoot"
					property-editor-ui-alias="Umb.PropertyEditorUi.Toggle"></umb-property>
				<umb-property
					label=${this.localize.term('blockEditor_allowBlockInAreas')}
					alias="allowInAreas"
					property-editor-ui-alias="Umb.PropertyEditorUi.Toggle"></umb-property>
			</uui-box>
			<uui-box headline=${this.localize.term('blockEditor_sizeOptions')}> ${this.#renderSizeOptions()} </uui-box>
		`;
	}

	#renderSizeOptions() {
		if (this._showSizeOptions) {
			return html`<umb-property
					label=${this.localize.term('blockEditor_allowedBlockColumns')}
					alias="columnSpanOptions"
					property-editor-ui-alias="Umb.PropertyEditorUi.BlockGridColumnSpan"></umb-property>
				<umb-property
					label=${this.localize.term('blockEditor_allowedBlockRows')}
					alias="availableRows"
					property-editor-ui-alias="Umb.PropertyEditorUi.NumberRange"></umb-property>`;
		} else {
			return html`<div id="showOptions">
				<uui-button
					label=${this.localize.term('blockEditor_showSizeOptions')}
					look="placeholder"
					@click=${() => (this._showSizeOptions = !this._showSizeOptions)}></uui-button>
			</div>`;
		}
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
				padding-bottom: var(--uui-size-layout-1); // To enforce some distance to the bottom of the scroll-container.
			}

			uui-box {
				margin-top: var(--uui-size-layout-1);
			}

			#showOptions {
				display: flex;
				height: 100px;
			}
			#showOptions uui-button {
				flex: 1;
			}
		`,
	];
}

export default UmbBlockTypeGridWorkspaceViewSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-type-workspace-view-settings': UmbBlockTypeGridWorkspaceViewSettingsElement;
	}
}
