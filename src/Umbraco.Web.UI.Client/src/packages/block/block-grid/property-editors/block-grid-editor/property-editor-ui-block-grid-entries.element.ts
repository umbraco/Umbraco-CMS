import { UmbBlockGridEntriesContext } from '../../context/block-grid-entries.context.js';
import type { UmbBlockGridLayoutModel } from '@umbraco-cms/backoffice/block';
import { html, customElement, state, repeat, css, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import '../../components/block-grid-block/block-grid-block.element.js';

/**
 * @element umb-property-editor-ui-block-grid-entries
 */
@customElement('umb-property-editor-ui-block-grid-entries')
export class UmbPropertyEditorUIBlockGridEntriesElement extends UmbLitElement {
	//
	// TODO: Make sure Sorter callbacks handles columnSpan when retrieving a new entry.

	#context = new UmbBlockGridEntriesContext(this);

	@property({ attribute: false })
	public set areaKey(value: string | null) {
		this.#context.setAreaKey(value);
	}
	public get areaKey(): string | null {
		return null; // Not implemented.
	}

	@state()
	private _layoutEntries: Array<UmbBlockGridLayoutModel> = [];

	@state()
	private _createButtonLabel = this.localize.term('blockEditor_addBlock');

	constructor() {
		super();
		this.observe(this.#context.layoutEntries, (layoutEntries) => {
			this._layoutEntries = layoutEntries;
		});
	}

	render() {
		// TODO: Missing ability to jump directly to creating a Block, when there is only one Block Type.
		return html` ${repeat(
				this._layoutEntries,
				(x) => x.contentUdi,
				(layoutEntry, index) =>
					html`<uui-button-inline-create
							href=${this.#context.getPathForCreateBlock(index) ?? ''}></uui-button-inline-create>
						<umb-property-editor-ui-block-grid-block data-udi=${layoutEntry.contentUdi} .layout=${layoutEntry}>
						</umb-property-editor-ui-block-grid-block>`,
			)}
			<uui-button-group>
				<uui-button
					id="add-button"
					look="placeholder"
					label=${this._createButtonLabel}
					href=${this.#context.getPathForCreateBlock(-1) ?? ''}></uui-button>
				<uui-button
					label=${this.localize.term('content_createFromClipboard')}
					look="placeholder"
					href=${this.#context.getPathForClipboard(-1) ?? ''}>
					<uui-icon name="icon-paste-in"></uui-icon>
				</uui-button>
			</uui-button-group>`;
		//
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: grid;
				gap: 1px;
			}
			> div {
				display: flex;
				flex-direction: column;
				align-items: stretch;
			}

			uui-button-group {
				padding-top: 1px;
				display: grid;
				grid-template-columns: 1fr auto;
			}
		`,
	];
}

export default UmbPropertyEditorUIBlockGridEntriesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-entries': UmbPropertyEditorUIBlockGridEntriesElement;
	}
}
