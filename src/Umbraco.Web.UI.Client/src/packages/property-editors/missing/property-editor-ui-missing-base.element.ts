import { css, html, nothing, property, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/content';
import { UmbDataTypeDetailRepository, type UmbDataTypeDetailModel } from '@umbraco-cms/backoffice/data-type';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';

export class UmbPropertyEditorUIMissingBaseElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value = '';

	@state()
	private _expanded = false;

	@query('#details')
	focalPointElement!: HTMLElement;

	private _dataTypeDetailModel?: UmbDataTypeDetailModel | undefined;
	private _dataTypeDetailRepository = new UmbDataTypeDetailRepository(this);

	protected _titleKey: string = '';
	protected _detailsDescriptionKey: string = '';
	protected _displayPropertyEditorUi: boolean = true;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT, (propertyContext) => {
			if (!propertyContext?.dataType) return;
			this.observe(propertyContext.dataType, (dt) => {
				if (!dt?.unique) return;
				this._updateEditorAlias(dt);
			});
		});
	}

	private async _updateEditorAlias(dataType: UmbPropertyTypeModel['dataType']) {
		this.observe(await this._dataTypeDetailRepository.byUnique(dataType.unique), (dataType) => {
			this._dataTypeDetailModel = dataType;
		});
	}

	async #onDetails() {
		this._expanded = !this._expanded;
		if (this._expanded) {
			await this.updateComplete;
			this.focalPointElement?.focus();
		}
	}

	override render() {
		return html`<uui-box id="info">
			<div slot="headline"><uui-icon id="alert" name="alert"></uui-icon>${this.localize.term(this._titleKey)}</div>
			<div id="content">
				<umb-localize key="missingEditor_description"></umb-localize>
				${this._expanded ? this._renderDetails() : nothing}
			</div>

			<uui-button
				id="details-button"
				compact
				label="${this.localize.term(this._expanded ? 'missingEditor_detailsHide' : 'missingEditor_detailsShow')}"
				@click=${this.#onDetails}>
				<span>${this.localize.term(this._expanded ? 'missingEditor_detailsHide' : 'missingEditor_detailsShow')}</span
				><uui-symbol-expand id="expand-symbol" .open=${this._expanded}></uui-symbol-expand>
			</uui-button>
		</uui-box>`;
	}

	private _renderDetails() {
		return html` <div id="details" tabindex="0">
			<umb-localize id="details-title" key="missingEditor_detailsTitle"></umb-localize>
			<p>
				<umb-localize key=${this._detailsDescriptionKey}></umb-localize>
			</p>
			<p>
				<strong><umb-localize key="missingEditor_detailsDataType"></umb-localize></strong>:
				<code>${this._dataTypeDetailModel?.name}</code><br />
				<strong><umb-localize key="missingEditor_detailsPropertyEditor"></umb-localize></strong>:
				<code>${this._dataTypeDetailModel?.editorAlias}</code>
				${this._displayPropertyEditorUi
					? html`
							<br />
							<strong><umb-localize key="missingEditor_detailsPropertyEditorUi"></umb-localize></strong>:
							<code>${this._dataTypeDetailModel?.editorUiAlias}</code>
						`
					: nothing}
			</p>
			<umb-code-block id="codeblock" copy language="${this.localize.term('missingEditor_detailsData')}"
				>${typeof this.value === 'object' ? JSON.stringify(this.value, null, 2) : String(this.value)}</umb-code-block
			>
		</div>`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
				--uui-box-default-padding: 0;
			}
			#content {
				padding: var(--uui-size-space-5);
				padding-bottom: var(--uui-size-space-3);
			}
			#alert {
				padding-right: var(--uui-size-space-2);
			}
			#details-button {
				float: right;
			}
			#details {
				margin-top: var(--uui-size-space-5);
			}
			#details-title {
				font-weight: 800;
			}
			#expand-symbol {
				transform: rotate(90deg);
			}
			#expand-symbol[open] {
				transform: rotate(180deg);
			}
			#codeblock {
				max-height: 400px;
				display: flex;
				flex-direction: column;
			}
		`,
	];
}
