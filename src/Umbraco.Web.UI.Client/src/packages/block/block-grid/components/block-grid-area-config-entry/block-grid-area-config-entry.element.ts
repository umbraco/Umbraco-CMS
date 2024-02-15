import { UmbBlockGridAreaConfigEntryContext } from './block-grid-area-config-entry.context.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, css, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import '../block-grid-block-view/index.js';

/**
 * @element umb-block-area-config-entry
 */
@customElement('umb-block-area-config-entry')
export class UmbBlockGridAreaConfigEntryElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	@property({ attribute: false })
	public get areaKey(): string | undefined {
		return this._areaKey;
	}
	public set areaKey(value: string | undefined) {
		if (!value) return;
		this._areaKey = value;
		this.setAttribute('data-area-key', value);
		this.#context.setAreaKey(value);
	}
	private _areaKey?: string | undefined;
	//

	#context = new UmbBlockGridAreaConfigEntryContext(this);

	@state()
	_columnSpan?: number;

	@state()
	_rowSpan?: number;

	@state()
	_alias = '';

	constructor() {
		super();

		// Misc:
		this.observe(this.#context.alias, (alias) => {
			this._alias = alias ?? '';
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		// element styling:
		this.observe(
			this.#context.columnSpan,
			(columnSpan) => {
				this._columnSpan = columnSpan;
				this.setAttribute('data-col-span', columnSpan ? columnSpan.toString() : '');
				this.style.setProperty('--umb-block-grid--grid-column', columnSpan ? columnSpan.toString() : '');
				this.style.setProperty('--umb-block-grid--area-column-span', columnSpan ? columnSpan.toString() : '');
			},
			'columnSpan',
		);
		this.observe(
			this.#context.rowSpan,
			(rowSpan) => {
				this._rowSpan = rowSpan;
				this.setAttribute('data-row-span', rowSpan ? rowSpan.toString() : '');
				this.style.setProperty('--umb-block-grid--area-row-span', rowSpan ? rowSpan.toString() : '');
			},
			'rowSpan',
		);
	}

	#renderBlock() {
		return this._areaKey
			? html`
					<div class="umb-block-grid-area-editor__area">
						<div>${this._alias}</div>
						<uui-action-bar>
							<uui-button label="edit" compact href=${'#edit_area_path_missing'}>
								<uui-icon name="icon-edit"></uui-icon>
							</uui-button>
							<uui-button label="delete" compact @click=${() => this.#context.requestDelete()}>
								<uui-icon name="icon-remove"></uui-icon>
							</uui-button>
						</uui-action-bar>
					</div>
			  `
			: '';
	}

	render() {
		return this.#renderBlock();
	}

	static styles = [
		css`
			:host {
				position: relative;
				display: block;
			}
			uui-action-bar {
				position: absolute;
				top: var(--uui-size-2);
				right: var(--uui-size-2);
			}

			:host([drag-placeholder]) {
				opacity: 0.2;
			}
		`,
	];
}

export default UmbBlockGridAreaConfigEntryElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-area-config-entry': UmbBlockGridAreaConfigEntryElement;
	}
}
