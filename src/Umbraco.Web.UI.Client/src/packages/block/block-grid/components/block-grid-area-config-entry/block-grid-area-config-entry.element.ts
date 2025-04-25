import { UmbBlockGridAreaConfigEntryContext } from './block-grid-area-config-entry.context.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, css, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import '../block-scale-handler/block-scale-handler.element.js';
/**
 * @element umb-block-area-config-entry
 */
@customElement('umb-block-area-config-entry')
export class UmbBlockGridAreaConfigEntryElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	@property({ attribute: false })
	public get key(): string | undefined {
		return this._key;
	}
	public set key(key: string | undefined) {
		if (!key) return;
		this._key = key;
		this.setAttribute('data-area-key', key);
		this.#context.setAreaKey(key);
	}
	private _key?: string | undefined;
	//

	@property()
	workspacePath?: string;

	#context = new UmbBlockGridAreaConfigEntryContext(this);

	@state()
	_columnSpan?: number;

	@state()
	_rowSpan?: number;

	@state()
	_alias = '';

	constructor() {
		super();

		this.observe(this.#context.alias, (alias) => {
			this._alias = alias ?? '';
		});
	}

	override connectedCallback(): void {
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
		return this._key
			? html`
					<span>${this._alias}</span>
					<uui-action-bar>
						<uui-button label="edit" compact href=${this.workspacePath + 'edit/' + this._key}>
							<uui-icon name="icon-edit"></uui-icon>
						</uui-button>
						<uui-button label="delete" compact @click=${() => this.#context.requestDelete()}>
							<uui-icon name="icon-remove"></uui-icon>
						</uui-button>
					</uui-action-bar>
					<umb-block-scale-handler @mousedown=${(e: MouseEvent) => this.#context.scaleManager.onScaleMouseDown(e)}>
						${this._columnSpan}x${this._rowSpan}
					</umb-block-scale-handler>
				`
			: '';
	}

	override render() {
		return this.#renderBlock();
	}

	// TODO: Update UUI, as it is missing proper colors to be used for this case:
	static override styles = [
		css`
			:host {
				position: relative;
				display: block;
				box-sizing: border-box;
				background-color: var(--uui-color-disabled);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				transition: background-color 120ms;
			}

			:host(:hover) {
				background-color: var(--uui-color-disabled-standalone);
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
